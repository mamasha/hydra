/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
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
    }

    class VcallSubsystem : IVcallSubsystem
    {
        #region members

        private static readonly ILogger _log = Logger.New<VcallSubsystem>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private IHostResolver _resolver;

        #endregion

        #region construction

        public static IVcallSubsystem New(VcallConfiguration config)
        {
            return
                new VcallSubsystem(config);
        }

        private VcallSubsystem(VcallConfiguration config)
        {
            _resolver = HostResolver.New(config);
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
        }

        IVhosting IVcallSubsystem.NewHosting(HostingConfiguration config)
        {
            var hosting = new HostingPeer(config);

            string callbackUri = hosting.ListeningUri;
            _resolver.PublishHostingPeer(config.ResolvingKey, callbackUri, hosting);

            return hosting;
        }

        IVtarget IVcallSubsystem.NewTarget(TargetConfiguration config)
        {
            var target = new TargetPeer(config);

            _resolver.SubscribeTargetPeer(config.ResolvingKey, target.OnHostingPeerDiscovery, target);

            return target;
        }

        #endregion
    }
}