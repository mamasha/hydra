/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.Discovery;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Target;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Core
{
    interface IVcallSubsystem
    {
        VcallConfiguration Config { get; }
        IHostResolver Resolver { get; }

        void Start();
        void Stop();

        IVhosting NewHosting(HostingConfiguration config);
        IVtarget NewTargets(TargetConfiguration config);

        void CloseHosting(ICommNode node);
        void CloseTargets(ICommNode node);

        DebugCounters DebugCounters { get; }
    }

    class VcallSubsystem : IVcallSubsystem
    {
        #region members

        private static readonly ILogger _log = Logger.New<VcallSubsystem>();
        private static readonly IHelpers Helpers = HelpersInUse.All;
        private static readonly Internal _internalAccess = new Internal();

        private readonly Object _mutex;
        private readonly VcallConfiguration _vconfig;
        private readonly IRepository _repo;
        private readonly IHostResolver _resolver;

        private DebugCounters _counters;

        #endregion

        #region construction

        public static IVcallSubsystem New(VcallConfiguration vconfig)
        {
            return
                new VcallSubsystem(vconfig);
        }

        private VcallSubsystem(VcallConfiguration vconfig)
        {
            var resolvingConfig = FillPropertiesOf<HostResolverConfiguration>.From(vconfig);

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

        protected string make_dynamic_uri(string tag, string role)
        {
            string hostname = "localhost";
            int port = _vconfig.Port ?? Helpers.FindAvailableTcpPort();

            return
                String.Format(_vconfig.CallbackUriPattern, hostname, port, role, tag);
        }

        #endregion

        #region private

        private DebugCounters accumulate_debug_counters()
        {
            var counters = new DebugCounters();

            counters.Accumulate(_counters);
            counters.Accumulate(_resolver.Counters);

            foreach (var node in _repo.GetNodes())
            {
                counters.Accumulate(node.DebugCounters);
            }

            return counters;
        }

        private HostingPeer new_hostring(HostingConfiguration config)
        {
            var timeout = TimeSpan.FromMilliseconds(_vconfig.Timeouts.HostingOpening);
            int addressInUseRetries = 0;

            for (;;)
            {
                var hosting = new HostingPeer(config, this);
                string uri = make_dynamic_uri(hosting.Tag, "hosting");

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

                        throw Helpers.MakeNew<VcallException>(ex, _log,
                            "hosting.{0}: Failed to listen on '{1}'; probably the TCP port is constantly in use (retries={2})", hosting.Tag, uri, addressInUseRetries);
                    }

                    throw;
                }
            }
        }

        private TargetsPeer new_targets(TargetConfiguration config)
        {
            var timeout = TimeSpan.FromMilliseconds(_vconfig.Timeouts.TargetOpening);
            int addressInUseRetries = 0;

            for (;;)
            {
                var targets = new TargetsPeer(config, this);
                string uri = make_dynamic_uri(targets.Tag, "targets");

                try
                {
                    targets.Start(uri, timeout);
                    return targets;
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

                        throw Helpers.MakeNew<VcallException>(ex, _log,
                            "targets.{0}: Failed to listen on '{1}'; probably the TCP port is constantly in use (retries={2})", targets.Tag, uri, addressInUseRetries);
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

            _resolver.Publish(callbackUri, CommNode.HostingRole, hosting.Tag);
            _resolver.Subscribe(hosting.OnTargetsDiscovery, hosting.Tag);

            lock (_mutex)
            {
                _repo.Add(hosting);
                _counters.Vcall_Event_NewHosting++;
            }

            return hosting;
        }

        IVtarget IVcallSubsystem.NewTargets(TargetConfiguration config)
        {
            var targets = new_targets(config);
            string callbackUri = targets.ListeningUri;

            _resolver.Publish(callbackUri, CommNode.TargetsRole, targets.Tag);
            _resolver.Subscribe(targets.OnHostingDiscovery, targets.Tag);

            lock (_mutex)
            {
                _repo.Add(targets);
                _counters.Vcall_Event_NewTargets++;
            }

            return targets;
        }

        void IVcallSubsystem.CloseHosting(ICommNode node)
        {
            throw
                Helpers.NewNotImplementedException();

            lock (_mutex)
            {
                _repo.Remove(node);
                _counters.Vcall_Event_CloseHosting++;
            }

//            _resolver.CancelPublishedHosting(node);
            close_comm_node(node);
        }

        void IVcallSubsystem.CloseTargets(ICommNode node)
        {
            throw
                Helpers.NewNotImplementedException();

            lock (_mutex)
            {
                _repo.Remove(node);
                _counters.Vcall_Event_CloseTargets++;
            }

//            _resolver.CancelSubscribedTarget(node);
            close_comm_node(node);
        }

        DebugCounters IVcallSubsystem.DebugCounters
        {
            get { return accumulate_debug_counters(); }
        }

        #endregion
    }
}