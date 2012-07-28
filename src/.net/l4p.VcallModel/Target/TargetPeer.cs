/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
using l4p.VcallModel.Core;
using l4p.VcallModel.Helpers;
using l4p.VcallModel.Hosting;

namespace l4p.VcallModel.Target
{
    class TargetPeer 
        : CommNode
        , ITargetPeer, IVtarget
    {
        #region members

        private static readonly ILogger _log = Logger.New<TargetPeer>();
        private static readonly IHelpers Helpers = LoggedHelpers.New(_log);

        private readonly Object _mutex;
        private readonly TargetConfiguration _config;
        private readonly IVcallSubsystem _core;
        private readonly IRepository _repo;

        private readonly IWcfTarget _wcf;
        private string _listeningUri;

        #endregion

        #region construction

        public TargetPeer(TargetConfiguration config, VcallSubsystem core)
        {
            _mutex = new Object();
            _config = config;
            _core = core;
            _repo = Repository.New();
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
            Helpers.Assert(_repo.HasHost(callbackUri) == false, "Host '{0}' is already in", callbackUri);

            var host = Helpers.TryCatch(
                () => HostingPeerProxy.New(callbackUri),
                ex => Helpers.ThrowNew<TargetPeerException>(ex, "target.{0}: Failed to create host at '{1}'", _tag, callbackUri));

            Helpers.TryCatch(
                () => host.RegisterTargetPeer(_tag, _listeningUri),
                ex => Helpers.ThrowNew<TargetPeerException>(ex, "target.{0}: Failed to register self at host '{1}'", _tag, callbackUri));

            lock(_mutex)
            {
                _repo.AddHost(callbackUri, host);
            }

            _counters.Target_ConnectedHosts++;
            trace("connected to host '{0}' (hosts={1})", callbackUri, _repo.HostsCount);
        }

        private void handle_dead_hosting(string callbackUri)
        {
            lock(_mutex)
            {
                _repo.RemoveHost(callbackUri);
            }

            _counters.Target_ConnectedHosts--;
            trace("disconnected from host '{0}' (hosts={1})", callbackUri, _repo.HostsCount);
        }

        #endregion

        #region public api

        public void Start(string uri, TimeSpan timeout)
        {
            trace("starting at uri '{0}'", uri);

            _listeningUri = Helpers.TryCatch(
                () => _wcf.Start(uri, timeout),
                ex => Helpers.ThrowNew<HostingPeerException>(ex, "Failed to start target.{0} at '{1}'", _tag, uri));

            trace("target is started");
        }

        public string Tag
        {
            get { return _tag; }
        }

        // [discovery thread]
        public void OnHostingPeerDiscovery(string callbackUri, bool alive)
        {
            if (alive)
                handle_new_hosting(callbackUri);
            else
                handle_dead_hosting(callbackUri);
        }

        #endregion

        #region protected api

        protected override void Stop(TimeSpan timeout)
        {
            _log.Trace("target.{0}: target is stopped", _tag);

            // notify all targets

            throw new NotImplementedException();
        }

        #endregion

        #region ITargetPeer

        void ITargetPeer.HostIsOpened(string hostTag)
        {
            throw new NotImplementedException();
        }

        void ITargetPeer.HostIsClosed(string hostTag)
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