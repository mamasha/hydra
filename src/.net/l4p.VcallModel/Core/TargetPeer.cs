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
    class TargetPeer 
        : CommNode
        , ITargetPeer, IVtarget
    {
        #region members

        private static readonly ILogger _log = Logger.New<TargetPeer>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private readonly TargetConfiguration _config;
        private readonly IVcallSubsystem _core;

        #endregion

        #region construction

        public TargetPeer(TargetConfiguration config, VcallSubsystem core)
        {
            _config = config;
            _core = core;
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("target.{0}: {1}", _tag, msg);
        }

        private void handle_new_hosting(string callbackUri)
        {
            _counters.Target_AliveHosts++;
            trace("'{0}' is alive", callbackUri);
        }

        private void handle_dead_hosting(string callbackUri)
        {
            _counters.Target_AliveHosts--;
            trace("'{0}' is dead", callbackUri);
        }

        #endregion

        #region public api

        public void Start()
        {
            trace("target is started");
        }

        public void OnHostingPeerDiscovery(string callbackUri, bool alive)
        {
            if (alive)
                handle_new_hosting(callbackUri);
            else
                handle_dead_hosting(callbackUri);
        }

        #endregion

        #region protected api

        protected override void Stop()
        {
            _log.Trace("target.{0}: target is stopped", _tag);
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
            _core.CloseTarget(this);
        }

        #endregion
    }
}