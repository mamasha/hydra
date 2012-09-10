/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
using l4p.VcallModel.Core;
using l4p.VcallModel.ProxyPeers;
using l4p.VcallModel.Utils;
using l4p.VcallModel.VcallSubsystems;

namespace l4p.VcallModel.InvocationBusses
{
    class InvocationBus : IProxy
    {
        #region members

        private static readonly ILogger _log = Logger.New<InvocationBus>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly ICommPeer _peer;
        private readonly IVcallSubsystem _vcall;
        private readonly ProxyConfiguration _config;
        private readonly DebugCounters _counters;

        #endregion

        #region construction

        public InvocationBus(ICommPeer peer, IVcallSubsystem vcall, ProxyConfiguration config)
        {
            _peer = peer;
            _vcall = vcall;
            _config = config;
            _counters = Context.Get<ICountersDb>().NewCounters();
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            if (_log.TraceIsOff)
                return;

            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("ibus.{0}: {1}", _peer.Tag, msg);
        }

        #endregion

        #region public api

        public void HandleNewHosting(HostingInfo info)
        {
            if (info.NameSpace != _config.NameSpace)
            {
                trace("Not a my namespace hosting.{0} (namespace='{1}'); mine='{2}'", info.Tag, info.NameSpace, _config.NameSpace);
                _counters.InvocationBus_Event_NotMyNamespace++;
                return;
            }

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