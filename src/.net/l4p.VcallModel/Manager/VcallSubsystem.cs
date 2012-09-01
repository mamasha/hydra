/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Diagnostics;
using System.IO;
using l4p.VcallModel.Configuration;
using l4p.VcallModel.Connectivity;
using l4p.VcallModel.Core;
using l4p.VcallModel.Discovery;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Proxy;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Manager
{
    interface IVcallSubsystem
    {
        VcallConfiguration Config { get; }

        void Start();
        void Stop();

        IHosting NewHosting(HostingConfiguration config = null);
        IProxy NewProxy(ProxyConfiguration config = null);

        void Close(ICommNode node);

        void RunInMyContext(Action action);

        DebugCounters Counters { get; }
    }

    class VcallSubsystem : IVcallSubsystem
    {
        #region members

        private static readonly ILogger _log = Logger.New<VcallSubsystem>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly ICountersDb _countersDb;
        private readonly Self _self;
        private readonly string _connectivityTag;
        private readonly IEngine _engine;

        #endregion

        #region construction

        public static IVcallSubsystem New(VcallConfiguration vconfig)
        {
            return
                new VcallSubsystem(vconfig);
        }

        public static IVcallSubsystem New(string resolvingKey)
        {
            var vconfig = new VcallConfiguration { ResolvingKey = resolvingKey };

            return
                new VcallSubsystem(vconfig);
        }

        public static IVcallSubsystem New()
        {
            var vconfig = new VcallConfiguration {ResolvingKey = Helpers.GetRandomName()};

            return
                new VcallSubsystem(vconfig);
        }

        private VcallSubsystem(VcallConfiguration vconfig)
        {
            _connectivityTag = Helpers.GetRandomName();

            Logger.Config = vconfig.Logging;
            _countersDb = CountersDb.New();

            using (Context.With(_countersDb))
            {
                _self = new Self();
                _engine = new Engine(_self);

                var resolvingConfig = FillPropertiesOf<HostResolverConfiguration>.From(vconfig);

                _self.mutex = new Object();
                _self.vconfig = vconfig;
                _self.repo = Repository.New();
                _self.resolver = HostResolver.New(resolvingConfig);
                _self.connectivity = ConnectivityManager.New();

                _self.counters = _countersDb.NewCounters();
            }
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            if (_log.TraceIsOff)
                return;

            string msg = Helpers.SafeFormat(format, args);
            _log.Trace(msg);
        }

        private void warn(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Warn(msg);
        }

        private void close_comm_nodes(ICommNode[] nodes)
        {
            int timeout = _self.vconfig.Timeouts.NodeClosing;
            int wcfTimeout = _self.vconfig.Timeouts.WcfHostClosing;

            var observer = DoneEvent.New(nodes.Length);

            foreach (var node in nodes)
            {
                trace("closing node.{0}", node.Tag);
                _engine.CloseNode(wcfTimeout, node, observer);
            }

            if (observer.Wait(timeout) == false)
            {
                int notReadyCount = observer.NotReadyCount;

                lock (_self.mutex)
                    _self.counters.Vcall_Error_CloseCommNode += notReadyCount;

                if (notReadyCount == 1)
                    warn("Failed to stop node.{0} (timeout={1})", nodes[0].Tag, timeout);
                else
                    warn("Failed to stop {0} nodes (timeout={1})", notReadyCount, timeout);
            }
            else
            {
                if (nodes.Length == 1)
                    trace("node.{0} is closed", nodes[0].Tag);
                else
                    trace("{0} nodes are closed", nodes.Length);
            }

        }

        #endregion

        #region IVcallSubsystem

        public VcallConfiguration Config
        {
            get { return _self.vconfig.Clone(); }
        }

        void IVcallSubsystem.Start()
        {
            _self.resolver.Start();
            _self.connectivity.Start();

            _self.resolver.Subscribe(_self.connectivity.NotifyPubSubMsg, _connectivityTag);
        }

        void IVcallSubsystem.Stop()
        {
            var nodes = _self.repo.GetNodes();

            if (nodes.Length > 0)
            {
                warn("Stop(): There are {0} active nodes", nodes.Length);
                close_comm_nodes(nodes);
            }

            _self.resolver.Cancel(_connectivityTag);

            _self.connectivity.Stop();
            _self.resolver.Stop();
        }

        IHosting IVcallSubsystem.NewHosting(HostingConfiguration config)
            // user arbitrary thread
        {
            if (config == null)
                config = new HostingConfiguration();

            if (config.ProxyRole == null) config.ProxyRole = _self.vconfig.ProxyRole;
            if (config.HostingRole == null) config.HostingRole = _self.vconfig.HostingRole;
            if (config.SubscribeToProxy_RetryTimeout == null) config.SubscribeToProxy_RetryTimeout = _self.vconfig.Timeouts.ProxyHostingSubscriptionRetry;

            HostingPeer hosting;

            using (Context.With(_countersDb))
            {
                hosting = _engine.NewHosting(config, this);
            }

            string callbackUri = hosting.ListeningUri;

            _self.resolver.Publish(callbackUri, config.HostingRole, hosting.Tag);
            _self.resolver.Subscribe(hosting.OnProxyDiscovery, hosting.Tag);

            lock (_self.mutex)
            {
                _self.repo.Add(hosting);
                _self.counters.Vcall_Event_NewHosting++;
            }

            trace("hostring.{0} is started", hosting.Tag);

            return hosting;
        }

        IProxy IVcallSubsystem.NewProxy(ProxyConfiguration config)
            // user arbitrary thread
        {
            if (config == null)
                config = new ProxyConfiguration();

            if (config.ProxyRole == null) config.ProxyRole = _self.vconfig.ProxyRole;
            if (config.HostingRole == null) config.HostingRole = _self.vconfig.HostingRole;
            if (config.NonRegisteredCall == NonRegisteredCallPolicy.Default) config.NonRegisteredCall = _self.vconfig.NonRegisteredCall;
            if (config.SubscribeToHosting_RetryTimeout == null) config.SubscribeToHosting_RetryTimeout = _self.vconfig.Timeouts.ProxyHostingSubscriptionRetry;

            ProxyPeer proxy;

            using (Context.With(_countersDb))
            {
                proxy = _engine.NewProxy(config, this);
            }

            string callbackUri = proxy.ListeningUri;

            _self.resolver.Publish(callbackUri, config.ProxyRole, proxy.Tag);
            _self.resolver.Subscribe(proxy.OnHostingDiscovery, proxy.Tag);

            lock (_self.mutex)
            {
                _self.repo.Add(proxy);
                _self.counters.Vcall_Event_NewProxy++;
            }

            trace("proxy.{0} is started", proxy.Tag);

            return proxy;
        }

        void IVcallSubsystem.Close(ICommNode node)
            // user arbitrary thread
        {
            close_comm_nodes(new[] {node});
        }

        void IVcallSubsystem.RunInMyContext(Action action)
        {
            using (Context.With(_countersDb))
            {
                action();
            }
        }

        DebugCounters IVcallSubsystem.Counters
        {
            get { return _countersDb.SumAll(); }
        }

        #endregion
    }
}