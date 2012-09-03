/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;
using l4p.VcallModel.VcallSubsystems;

namespace l4p.VcallModel.InvokationBusses
{
    class InvokationBus : IProxy
    {
        #region members

        private static readonly ILogger _log = Logger.New<InvokationBus>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly ICommPeer _peer;
        private readonly IVcallSubsystem _vcall;
        private readonly ProxyConfiguration _config;

        #endregion

        #region construction

        public static IProxy New(ICommPeer peer, IVcallSubsystem vcall, ProxyConfiguration config)
        {
            return
                new InvokationBus(peer, vcall, config);
        }

        private InvokationBus(ICommPeer peer, IVcallSubsystem vcall, ProxyConfiguration config)
        {
            _peer = peer;
            _vcall = vcall;
            _config = config;
        }

        internal void HandlePublisher()
        {
            throw
                Helpers.NewNotImplementedException();
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

        #region IProxy

        void IProxy.Call(Expression<Action> vcall)
        {
            throw new NotImplementedException();
        }

        R IProxy.Call<R>(Expression<Func<R>> vcall)
        {
            throw new NotImplementedException();
        }

        void IProxy.Call(string methodName, params object[] args)
        {
            throw new NotImplementedException();
        }

        R IProxy.Call<R>(string functionName, params object[] args)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}