﻿/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Configuration;
using l4p.VcallModel.Core;
using l4p.VcallModel.Discovery;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Target;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Manager
{
    interface IVcallSubsystem
    {
        VcallConfiguration Config { get; }

        void Start();
        void Stop();

        IVhosting NewHosting(HostingConfiguration config = null);
        IVtarget NewTargets(TargetConfiguration config = null);

        void Close(ICommNode node);

        DebugCounters Counters { get; }
    }

    class VcallSubsystem : IVcallSubsystem
    {
        #region members

        private static readonly ILogger _log = Logger.New<VcallSubsystem>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly ICountersDb _countersDb;
        private readonly Self _self;
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
            var vconfig = new VcallConfiguration {ResolvingKey = Helpers.RandomName8()};

            return
                new VcallSubsystem(vconfig);
        }

        private VcallSubsystem(VcallConfiguration vconfig)
        {
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

            var observer = DoneEvent.New(nodes.Length);

            foreach (var node in nodes)
            {
                _engine.CloseNode(timeout, node, observer);
            }

            if (observer.Wait(timeout) == false)
            {
                int notReadyCount = observer.NotReadyCount;

                lock (_self.mutex)
                    _self.counters.Vcall_Error_CloseCommNode += notReadyCount;

                warn("Failed to stop {0} nodes", notReadyCount);
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
        }

        void IVcallSubsystem.Stop()
        {
            var nodes = _self.repo.GetNodes();

            if (nodes.Length > 0)
            {
                warn("Stop(): There are {0} active nodes", nodes.Length);
                close_comm_nodes(nodes);
            }

            _self.resolver.Stop();
        }

        IVhosting IVcallSubsystem.NewHosting(HostingConfiguration config)
            // user arbitrary thread
        {
            if (config == null)
                config = new HostingConfiguration();

            if (config.TargetsRole == null) config.TargetsRole = _self.vconfig.TargetsRole;
            if (config.HostingRole == null) config.HostingRole = _self.vconfig.HostingRole;
            if (config.SubscribeToTargets_RetryTimeout == null) config.SubscribeToTargets_RetryTimeout = _self.vconfig.Timeouts.TargetsHostingSubscription;

            HostingPeer hosting;

            using (Context.With(_countersDb))
            {
                hosting = _engine.NewHosting(config, this);
            }

            string callbackUri = hosting.ListeningUri;

            _self.resolver.Publish(callbackUri, config.HostingRole, hosting.Tag);
            _self.resolver.Subscribe(hosting.OnTargetsDiscovery, hosting.Tag);

            lock (_self.mutex)
            {
                _self.repo.Add(hosting);
                _self.counters.Vcall_Event_NewHosting++;
            }

            trace("hostring.{0} is started", hosting.Tag);

            return hosting;
        }

        IVtarget IVcallSubsystem.NewTargets(TargetConfiguration config)
            // user arbitrary thread
        {
            if (config == null)
                config = new TargetConfiguration();

            if (config.TargetsRole == null) config.TargetsRole = _self.vconfig.TargetsRole;
            if (config.HostingRole == null) config.HostingRole = _self.vconfig.HostingRole;
            if (config.NonRegisteredCall == NonRegisteredCallPolicy.Default) config.NonRegisteredCall = _self.vconfig.NonRegisteredCall;
            if (config.SubscribeToHosting_RetryTimeout == null) config.SubscribeToHosting_RetryTimeout = _self.vconfig.Timeouts.TargetsHostingSubscription;

            TargetsPeer targets;

            using (Context.With(_countersDb))
            {
                targets = _engine.NewTargets(config, this);
            }

            string callbackUri = targets.ListeningUri;

            _self.resolver.Publish(callbackUri, config.TargetsRole, targets.Tag);
            _self.resolver.Subscribe(targets.OnHostingDiscovery, targets.Tag);

            lock (_self.mutex)
            {
                _self.repo.Add(targets);
                _self.counters.Vcall_Event_NewTargets++;
            }

            trace("targets.{0} is started", targets.Tag);

            return targets;
        }

        void IVcallSubsystem.Close(ICommNode node)
            // user arbitrary thread
        {
            close_comm_nodes(new[] {node});
        }

        DebugCounters IVcallSubsystem.Counters
        {
            get { return _countersDb.SumAll(); }
        }

        #endregion
    }
}