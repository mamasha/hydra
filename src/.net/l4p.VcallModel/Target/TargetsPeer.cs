/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
using System.ServiceModel;
using l4p.VcallModel.Core;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Target
{
    class TargetsPeer 
        : CommNode
        , ITargetsPeer, IVtarget
    {
        #region members

        private static readonly ILogger _log = Logger.New<TargetsPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly TargetConfiguration _config;
        private readonly IVcallSubsystem _core;
        private readonly IRepository _repo;

        private readonly IWcfTarget _wcf;
        private readonly IActiveThread _thr;

        private string _listeningUri;

        #endregion

        #region construction

        public TargetsPeer(TargetConfiguration config, VcallSubsystem core)
        {
            _config = config;
            _core = core;
            _repo = Repository.New();
            _wcf = new WcfTarget(this);
            _thr = ActiveThread.New(String.Format("targets.{0}", _tag));
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("targets.{0}: {1}", _tag, msg);
        }

        private void try_catch(Action action, Action onError, string errFmt, params object[] args)
        {
            try { action(); }
            catch (Exception ex)
            {
                onError();
                throw Helpers.MakeNew<TargetsPeerException>(ex, _log, errFmt, args);
            }
        }

        private R try_catch<R>(Func<R> func, Action onError, string errFmt, params object[] args)
        {
            try { return func(); }
            catch (Exception ex)
            {
                onError();
                throw Helpers.MakeNew<TargetsPeerException>(ex, _log, errFmt, args);
            }
        }

        private void subsribe_self_to_hosting(string callbackUri, TargetsInfo myInfo)
        {
            var hosting = try_catch(
                () => HostingPeerProxy.New(callbackUri), 
                () => ++_counters.Targets_Error_SubscribeToHosting, "targets.{0}: Failed to create proxy at '{1}'", _tag, callbackUri);

            try_catch(
                () => hosting.SubscribeTargets(myInfo),
                () => ++_counters.Targets_Error_SubscribeToHosting, "targets.{0}: Failed to register self at host '{1}'", _tag, callbackUri);

            _counters.Targets_Event_SubscribeSelfToHosting++;
            trace("subscribed to host '{0}' (hosts={1})", callbackUri, _repo.AliveCount);
        }

        private void handle_alive_hosting(string callbackUri)
        {
            var myInfo = new TargetsInfo
            {
                Tag = _tag,
                ListeningUri = _listeningUri,
                NameSpace = _config.NameSpace,
                HostName = Helpers.GetLocalhostFqdn()
            };

            _thr.DoOnce(_subscribe_RetryTimeout, callbackUri,
                () => subsribe_self_to_hosting(callbackUri, myInfo), "subscribe targets.{0} to {1}", _tag, callbackUri);
        }

        private void handle_dead_hosting(string callbackUri)
        {
            _thr.Cancel(callbackUri);
            _repo.CleanUp(callbackUri);

            trace("host at '{0}' is dead; aliveHosts={1})", callbackUri, _repo.AliveCount);
        }

        private void subscribe_hosting(HostingInfo info)
        {
            trace("Got a new hosting.{0} at '{1}'", info.Tag, info.CallbackUri);

            if (_repo.HasHosting(info))
            {
                trace("hosting.{0} at {1} is already registered", info.Tag, info.CallbackUri);
                _counters.Targets_Event_AlreadyHereHosting++;
                return;
            }

            _repo.AddHosting(info);
            _counters.Targets_Event_SubscribeHosing++;
        }

        private void cancel_hosting(string hostingUri)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        #endregion

        #region public api

        public void Start(string uri, TimeSpan timeout)
        {
            trace("starting at uri '{0}'", uri);

            _listeningUri = Helpers.TryCatch(_log,
                () => _wcf.Start(uri, timeout),
                ex => Helpers.ThrowNew<HostingPeerException>(ex, _log, "Failed to start targets.{0} at '{1}'", _tag, uri));

            _thr.Start();

            trace("target is started");
        }

        public string Tag
        {
            get { return _tag; }
        }

        public void OnHostingDiscovery(string callbackUri, string role, bool alive)
            // discovery thread
        {
            if (role != HostingRole)
                return;

            if (alive)
            {
                _thr.PostAction(
                    () => handle_alive_hosting(callbackUri));

                _counters.Targets_Event_HostIsAlive++;
            }
            else
            {
                _thr.PostAction(
                    () => handle_dead_hosting(callbackUri));

                _counters.Targets_Event_HostIsDead++;
            }
        }

        public string ListeningUri
        {
            get { return _listeningUri; }
        }

        #endregion

        #region protected api

        protected override void Stop(TimeSpan timeout)
        {
            trace("target is stopped");

            // notify all hostings

            _wcf.Stop(timeout);
            _thr.Stop();

            trace("targets is stopped");
        }

        #endregion

        #region ITargetsPeer

        void ITargetsPeer.SubscribeHosting(HostingInfo info)
            // one way message; arbitrary WCF thread
        {
            _thr.PostAction(
                () => subscribe_hosting(info), "Sibscribing hosting.{0} peer at '{1}'", info.Tag, info.CallbackUri);
        }

        void ITargetsPeer.CancelHosting(string hostingTag)
            // one way message; arbitrary WCF thread
        {
            _thr.PostAction(
                () => cancel_hosting(hostingTag), "Canceling hosting.{0}", hostingTag);
        }

        #endregion

        #region IVtarget

        void IVtarget.Call(Expression<Action> vcall)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        R IVtarget.Call<R>(Expression<Func<R>> vcall)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVtarget.Call(string methodName, params object[] args)
        {
            throw
                Helpers.MakeNew<VcallException>(null, _log, "'{0}' There is no registered targets for subject '{1}'", "resolving key", methodName);

            throw
                Helpers.NewNotImplementedException();
        }

        R IVtarget.Call<R>(string functionName, params object[] args)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVtarget.Close()
        {
            _core.CloseTargets(this);
        }

        #endregion
    }
}