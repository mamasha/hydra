/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
using l4p.VcallModel.Core;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Manager;
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

            var thrConfig = new ActiveThread.Config
            {
                Name = String.Format("targets.{0}", _tag),
                FailureTimeout = core.Config.Timeouts.ActiveThread_FailureTimeout
            };

            _thr = ActiveThread.New(thrConfig);
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            if (_log.TraceIsOff)
                return;

            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("targets.{0}: {1}", _tag, msg);
        }

        private void warn(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Warn("targets.{0}: {1}", _tag, msg);
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
                () => Proxies.HostingPeer.New(callbackUri), 
                () => ++_counters.Targets_Error_SubscribeToHosting, "targets.{0}: Failed to create proxy at '{1}'", _tag, callbackUri);

            try_catch(
                () => hosting.SubscribeTargets(myInfo),
                () => ++_counters.Targets_Error_SubscribeToHosting, "targets.{0}: Failed to register self at host '{1}'", _tag, callbackUri);

            _counters.Targets_Event_SubscribedToHosting++;
            trace("subscribed to host '{0}' (hosts={1})", callbackUri, _repo.HostingCount);
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

            _thr.DoOnce(_config.SubscribeToHosting_RetryTimeout.Value, callbackUri,
                () => subsribe_self_to_hosting(callbackUri, myInfo), "subscribe targets.{0} to {1}", _tag, callbackUri);
        }

        private void handle_dead_hosting(string callbackUri)
        {
            _thr.Cancel(callbackUri);

            var info = _repo.FindHosting(callbackUri);

            if (info == null)
                return;

            _repo.RemoveHosting(info);

            trace("host at '{0}' is dead; aliveHosts={1})", callbackUri, _repo.HostingCount);
        }

        private void subscribe_hosting(HostingInfo info)
        {
            trace("Got a new hosting.{0} at '{1}'", info.Tag, info.CallbackUri);

            if (_repo.HasHosting(info.Tag))
            {
                trace("hosting.{0} at {1} is already registered", info.Tag, info.CallbackUri);
                _counters.Targets_Event_KnownHosing++;
                return;
            }

            info.Proxy = Proxies.HostingPeer.New(info.CallbackUri);

            _repo.AddHosting(info);
            _counters.Targets_Event_NewHosing++;
        }

        private void cancel_hosting(string hostingTag)
        {
            var info = _repo.FindHosting(hostingTag);

            if (info == null)
            {
                trace("unknown hosting.{0}", hostingTag);
                _counters.Targets_Event_UnknownHosing++;
                return;
            }

            _repo.RemoveHosting(info);
            _counters.Targets_Event_CanceledHosting++;

            trace("hosting.{0} is canceled", hostingTag);
        }

        private void stop(TimeSpan timeout, IDoneEvent observer)
        {
            if (_state == State.Stopped)
            {
                _counters.Targets_Event_IsAlreadyStopped++;
                return;
            }

            _state = State.Stopped;

            var hostings = _repo.GetHostings();

            foreach (var hosting in hostings)
            {
                _repo.RemoveHosting(hosting);

                try
                {
                    hosting.Proxy.CancelTargets(_tag);
                }
                catch (Exception ex)
                {
                    warn("Failed to cancel hosting.{0} gracefully; {1}", hosting.Tag, ex.GetDetailedMessage());
                }
            }

            _wcf.Stop(timeout);
            _thr.Stop(() => stop_tail(observer));
        }

        private void stop_tail(IDoneEvent observer)
        {
            _counters.Targets_Event_IsStopped++;
            observer.Signal();

            trace("hosting is stopped");
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

            _state = State.Started;
            _counters.Targets_Event_IsStarted++;

            trace("targets is started");
        }

        public string Tag
        {
            get { return _tag; }
        }

        public void OnHostingDiscovery(string callbackUri, string role, bool alive)
            // discovery thread
        {
            if (role != _config.HostingRole)
                return;

            if (alive)
            {
                _thr.PostAction(
                    () => handle_alive_hosting(callbackUri));

                _counters.Targets_Event_AliveHosting++;
            }
            else
            {
                _thr.PostAction(
                    () => handle_dead_hosting(callbackUri));

                _counters.Targets_Event_DeadHosting++;
            }
        }

        public string ListeningUri
        {
            get { return _listeningUri; }
        }

        #endregion

        #region protected api

        protected override void Close()
        {
            _core.Close(this);
        }

        protected override void Stop(TimeSpan timeout, IDoneEvent observer)
            // user arbitrary thread
        {
            _thr.PostAction(
                () => stop(timeout, observer), "stopping hosting.{0}", _tag);
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

        #endregion
    }
}