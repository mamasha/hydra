/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Discovery;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Core
{
    interface IVcallSubsystem
    {
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
        private static readonly IHelpers Helpers = Utils.New(_log);
        private static readonly Internal _internalAccess = new Internal();

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

        private VcallSubsystem(VcallConfiguration config)
        {
            _repo = Repository.New();
            _resolver = HostResolver.New(config);
            _counters = new DebugCounters();
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

        #endregion

        #region IVcallSubsystem

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
                node.Stop(_internalAccess);
            }
        }

        IVhosting IVcallSubsystem.NewHosting(HostingConfiguration config)
        {
            var hosting = new HostingPeer(config, this);

            string callbackUri = hosting.ListeningUri;
            _resolver.PublishHostingPeer(callbackUri, hosting);

            _repo.Add(hosting);
            _counters.HostingsOpened++;

            return hosting;
        }

        IVtarget IVcallSubsystem.NewTarget(TargetConfiguration config)
        {
            var target = new TargetPeer(config, this);

            _resolver.SubscribeTargetPeer(target.OnHostingPeerDiscovery, target);

            _repo.Add(target);
            _counters.TargetsOpened++;

            return target;
        }

        void IVcallSubsystem.CloseHosting(ICommNode node)
        {
            _repo.Remove(node);

            _resolver.CancelPublishedHosting(node);
            node.Stop(_internalAccess);

            _counters.HostingsClosed++;
        }

        void IVcallSubsystem.CloseTarget(ICommNode node)
        {
            _repo.Remove(node);

            _resolver.CancelSubscribedTarget(node);
            node.Stop(_internalAccess);

            _counters.TargetsClosed++;
        }

        DebugCounters IVcallSubsystem.DebugCounters
        {
            get { return accumulate_debug_counters(); }
        }

        #endregion
    }
}