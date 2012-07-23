/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Core
{
    class TargetPeer : ITargetPeer, IVtarget
    {
        #region members

        private static readonly ILogger _log = Logger.New<TargetPeer>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private readonly TargetConfiguration _config;

        #endregion

        #region construction

        public TargetPeer(TargetConfiguration config)
        {
            _config = config;
        }

        #endregion

        #region public api

        public void OnHostingPeerDiscovery(string callbackUri, bool alive)
        {
            _log.Trace("[{0}] '{1}' is {2}", _config.ResolvingKey, callbackUri, alive ? "alive" : "dead");
        }

        #endregion

        #region IDisposable

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITargetPeer

        void ITargetPeer.UpdateSubjects()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVtarget

        void IVtarget.Call(Expression<Action> vcall)
        {
            throw new NotImplementedException();
        }

        R IVtarget.Call<R>(Expression<Func<R>> vcall)
        {
            throw new NotImplementedException();
        }

        void IVtarget.Call(string methodName, params object[] args)
        {
            throw
                Helpers.MakeNew<VcallException>(null, "'{0}' There is no registered targets for subject '{1}'", "resolving key", methodName);

            throw new NotImplementedException();
        }

        R IVtarget.Call<R>(string functionName, params object[] args)
        {
            throw new NotImplementedException();
        }

        void IVtarget.Close()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}