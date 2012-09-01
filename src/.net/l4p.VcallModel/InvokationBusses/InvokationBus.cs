/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.InvokationBusses
{
    class InvokationBus : IProxy
    {
        #region members

        private static readonly ILogger _log = Logger.New<InvokationBus>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private ProxyConfiguration _config;

        #endregion

        #region construction

        public static IProxy New(ProxyConfiguration config)
        {
            return
                new InvokationBus(config);
        }

        private InvokationBus(ProxyConfiguration config)
        {
            _config = config;
        }

        #endregion

        #region ICommNode

        string ICommNode.Tag
        {
            get { throw new NotImplementedException(); }
        }

        void ICommNode.Close()
        {
            throw new NotImplementedException();
        }

        void ICommNode.Stop(Internal access, int timeout, IDoneEvent observer)
        {
            throw new NotImplementedException();
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