/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.Discovery;
using l4p.VcallModel.Helpers;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Target;

namespace l4p.VcallModel.Core
{
    interface IVcallSubsystem
    {
        VcallConfiguration Config { get; }
        IHostResolver Resolver { get; }

        void Start();
        void Stop();

        IVhosting NewHosting(HostingConfiguration config);
        IVtarget NewTarget(TargetConfiguration config);

        void CloseHosting(ICommNode node);
        void CloseTarget(ICommNode node);

        DebugCounters DebugCounters { get; }
    }

    class VcallSubsystem : IVcallSubsystem
    {
        #region members

        private static readonly ILogger _log = Logger.New<VcallSubsystem>();
        private static readonly IHelpers Helpers = LoggedHelpers.New(_log);
        private static readonly Internal _internalAccess = new Internal();

        private readonly Object _mutex;
        private readonly VcallConfiguration _vconfig;
        private readonly IRepository _repo;
        private readonly IHostResolver _resolver;

        private DebugCounters _counters;

        #endregion

        #region construction

        public static IVcallSubsystem New(VcallConfiguration config)
        {
            return
                new VcallSubsystem(config);
        }

        private VcallSubsystem(VcallConfiguration vconfig)
        {
            var resolvingConfig = new HostResolverConfiguration
            {
                ResolvingKey = vconfig.ResolvingKey,
                HelloMessageGap = vconfig.Timeouts.HelloMessageGap,
                ByeMessageGap = vconfig.Timeouts.ByeMessageGap,
                DiscoveryScopePattern = vconfig.DiscoveryScopePattern,
                DiscoveryOpening = vconfig.Timeouts.DiscoveryOpening,
                DiscoveryClosing = vconfig.Timeouts.DiscoveryClosing
            };

            _mutex = new Object();
            _vconfig = vconfig;
            _repo = Repository.New();
            _resolver = HostResolver.New(resolvingConfig);
            _counters = new DebugCounters();
        }

        private void close_comm_node(ICommNode node)
        {
            var timeout = new TimeSpan(_vconfig.Timeouts.HostingClosing);
            node.Stop(_internalAccess, timeout);
        }

        protected string make_dynamic_uri(string tag)
        {
            string hostname = "localhost";
            int port = _vconfig.Port ?? Helpers.FindAvailableTcpPort();

            return
                String.Format(_vconfig.HostingUriPattern, hostname, port, tag);
        }

        #endregion

        #region private

        private DebugCounters accumulate_debug_counters()
        {
            var counters = new DebugCounters();

            counters.Accumulate(_counters);
            counters.Accumulate(_resolver.DebugCounters);

            foreach (var node in _repo.GetNodes())
            {
                counters.Accumulate(node.DebugCounters);
            }

            return counters;
        }

        private IVhosting new_hostring(HostingConfiguration config)
        {
            var timeout = Helpers.TimeSpanFromMillis(_vconfig.Timeouts.HostingOpening);
            int addressInUseRetries = 0;

            for (;;)
            {
                var hosting = new HostingPeer(config, this);
                string uri = make_dynamic_uri(hosting.Tag);

                try
                {
                    hosting.Start(uri, timeout);
                    return hosting;
                }
                catch (Exception ex)
                {
                    if (ex.IsConsequenceOf<AddressAlreadyInUseException>())
                    {
                        addressInUseRetries++;

                        if (addressInUseRetries <= _vconfig.AddressInUseRetries)
                        {
                            _log.Warn("Dynamic URI '{0}' is in use; trying other one (retries={1})", uri, addressInUseRetries);
                            continue;
                        }

                        throw Helpers.MakeNew<VcallException>(ex, 
                            "host.{0}: Failed to listen on '{1}'; probably the TCP port is constantly in use (retries={2})", hosting.Tag, uri, addressInUseRetries);
                    }

                    throw;
                }
            }
        }

        private TargetPeer new_target(TargetConfiguration config)
        {
            var timeout = Helpers.TimeSpanFromMillis(_vconfig.Timeouts.TargetOpening);
            int addressInUseRetries = 0;

            for (;;)
            {
                var target = new TargetPeer(config, this);
                string uri = make_dynamic_uri(target.Tag);

                try
                {
                    target.Start(uri, timeout);
                    return target;
                }
                catch (Exception ex)
                {
                    if (ex.IsConsequenceOf<AddressAlreadyInUseException>())
                    {
                        addressInUseRetries++;

                        if (addressInUseRetries <= _vconfig.AddressInUseRetries)
                        {
                            _log.Warn("Dynamic URI '{0}' is in use; trying other one (retries={1})", uri, addressInUseRetries);
                            continue;
                        }

                        throw Helpers.MakeNew<VcallException>(ex,
                            "target.{0}: Failed to listen on '{1}'; probably the TCP port is constantly in use (retries={2})", target.Tag, uri, addressInUseRetries);
                    }

                    throw;
                }
            }
        }

        #endregion

        #region IVcallSubsystem

        public VcallConfiguration Config
        {
            get { return _vconfig.Clone(); }
        }

        IHostResolver IVcallSubsystem.Resolver
        {
            get { return _resolver; }
        }

        void IVcallSubsystem.Start()
        {
            _resolver.Start();
        }

        void IVcallSubsystem.Stop()
        {
            _resolver.Stop();

            var nodes = _repo.GetNodes();

            if (nodes.Length > 0)
            {
                _log.Warn("Stop(): There are {0} active nodes", nodes.Length);
            }

            foreach (var node in nodes)
            {
                close_comm_node(node);
            }
        }

        IVhosting IVcallSubsystem.NewHosting(HostingConfiguration config)
        {
            var hosting = new_hostring(config);
            string callbackUri = hosting.ListeningUri;

            _resolver.PublishHostingPeer(callbackUri, hosting);

            lock (_mutex)
            {
                _repo.Add(hosting);
                _counters.HostingsOpened++;
            }

            return hosting;
        }

        IVtarget IVcallSubsystem.NewTarget(TargetConfiguration config)
        {
            var target = new_target(config);

            _resolver.SubscribeTargetPeer(target.OnHostingPeerDiscovery, target);

            lock (_mutex)
            {
                _repo.Add(target);
                _counters.TargetsOpened++;
            }

            return target;
        }

        void IVcallSubsystem.CloseHosting(ICommNode node)
        {
            lock (_mutex)
            {
                _repo.Remove(node);
                _counters.HostingsClosed++;
            }

            _resolver.CancelPublishedHosting(node);
            close_comm_node(node);
        }

        void IVcallSubsystem.CloseTarget(ICommNode node)
        {
            lock (_mutex)
            {
                _repo.Remove(node);
                _counters.TargetsClosed++;
            }

            _resolver.CancelSubscribedTarget(node);
            close_comm_node(node);
        }

        DebugCounters IVcallSubsystem.DebugCounters
        {
            get { return accumulate_debug_counters(); }
        }

        #endregion
    }
}