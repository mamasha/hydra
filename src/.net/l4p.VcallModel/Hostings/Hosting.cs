/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;
using l4p.VcallModel.VcallSubsystems;

namespace l4p.VcallModel.Hostings
{
    class Hosting : IHosting
    {
        #region members

        private static readonly ILogger _log = Logger.New<Hosting>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly ICommPeer _peer;
        private readonly IVcallSubsystem _vcall;
        private readonly HostingConfiguration _config;

        #endregion

        #region construction

        public static IHosting New(ICommPeer peer, IVcallSubsystem vcall, HostingConfiguration config)
        {
            return
                new Hosting(peer, vcall, config);
        }

        private Hosting(ICommPeer peer, IVcallSubsystem vcall, HostingConfiguration config)
        {
            _peer = peer;
            _vcall = vcall;
            _config = config;
        }

        #endregion

        #region ICommNode

        string ICommNode.Tag
        {
            get { return _peer.Tag; }
        }

        void ICommNode.Close()
        {
            _vcall.Close(_peer);
        }

        #endregion

        #region IHosting

        void IHosting.Host(Action action)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IHosting.Host<R>(Func<R> func)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IHosting.Host<T1, T2>(string actionName, Action<T1, T2> action)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        R IHosting.Host<T1, T2, R>(string funcName, Func<T1, T2, R> func)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IHosting.Host<T1, T2>(Action<T1, T2> action)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IHosting.Host<T1, R>(Func<T1, R> func)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IHosting.Host<T1, T2, R>(Func<T1, T2, R> func)
        {
            throw Helpers.NewNotImplementedException();
        }

        string IHosting.ListeningUri
        {
            get { throw Helpers.NewNotImplementedException(); }
        }

        #endregion
    }
}