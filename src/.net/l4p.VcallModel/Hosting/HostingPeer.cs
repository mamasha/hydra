﻿/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Core;
using l4p.VcallModel.Target;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Hosting
{
    class HostingPeer 
        : CommNode
        , IHostingPeer, IVhosting
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostingPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly HostingConfiguration _config;
        private readonly IVcallSubsystem _core;

        private readonly IRepository _repo;
        private readonly IWcfHostring _wcf;
        private readonly IActiveThread _thr;

        private string _listeningUri;

        #endregion

        #region construction

        public HostingPeer(HostingConfiguration config, VcallSubsystem core)
        {
            _config = config;
            _core = core;
            _repo = Repository.New(this);
            _wcf = new WcfHosting(this);
            _thr = ActiveThread.New(String.Format("hosting.{0}", _tag));
        }

        #endregion

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("hosting.{0}: {1}", _tag, msg);
        }

        private void warn(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Warn("hosting.{0}: {1}", _tag, msg);
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

        private void handle_alive_targets(string callbackUri)
        {
            var myInfo = new HostingInfo
            {
                Tag = _tag,
                CallbackUri = _listeningUri,
                NameSpace = _config.NameSpace,
                HostName = Helpers.GetLocalhostFqdn()
            };

            _thr.DoOnce(_subscribe_RetryTimeout, callbackUri,
                () => subsribe_self_to_targets(callbackUri, myInfo), "subscribe hosting.{0} to {1}", _tag, callbackUri);
        }

        private void handle_dead_targets(string callbackUri)
        {
            _thr.Cancel(callbackUri);
            _repo.CleanUp(callbackUri);

            trace("targets at '{0}' is dead (targets={1})", callbackUri, _repo.TargetsCount);
        }

        private void subsribe_self_to_targets(string callbackUri, HostingInfo myInfo)
        {
            var targets = try_catch(
                () => TargetsPeerProxy.New(callbackUri),
                () => ++_counters.Hosting_Error_SubscribeSelfToTargets, "hosting.{0}: Failed to create proxy at '{1}'", _tag, callbackUri);

            try_catch(
                () => targets.SubscribeHosting(myInfo),
                () => ++_counters.Hosting_Error_SubscribeSelfToTargets, "hosting.{0}: Failed to subscribe self at host '{1}'", _tag, callbackUri);

            _counters.Hosting_Event_SubscribeSelfToTargets++;
            trace("subscribed to targets '{0}' (targets={1})", callbackUri, _repo.TargetsCount);
        }

        private void update_targets_peer(TargetsInfo info)
        {
            // generate update messages
            int count = 0;

            trace("{0} update messages are generated", count);
        }

        private void subscribe_targets(TargetsInfo info)
        {
            trace("Got a new targets.{0} at '{1}'", info.Tag, info.ListeningUri);

            if (_repo.HasTargets(info))
            {
                trace("targets.{0} at {1} is already registered", info.Tag, info.ListeningUri);
                _counters.Hosting_Event_AlreadyHereTargets++;
                return;
            }

            _repo.AddTargets(info);
            _counters.Hosting_Event_SubscribeTargets++;

            update_targets_peer(info);
        }

        private void cancel_targets(string targetsTag)
        {
            trace("targets.{0} is canceled", targetsTag);
        }

        #region public api

        public void Start(string uri, TimeSpan timeout)
        {
            trace("starting at uri '{0}'", uri);

            _listeningUri = Helpers.TryCatch(_log,
                () => _wcf.Start(uri, timeout),
                ex => Helpers.ThrowNew<HostingPeerException>(ex, _log, "Failed to start hosting.{0} at '{1}'", _tag, uri));

            _thr.Start();

            trace("host is started");
        }

        public string Tag
        {
            get { return _tag; }
        }

        public void OnTargetsDiscovery(string callbackUri, string role, bool alive)
            // discovery thread
        {
            if (role != TargetsRole)
                return;

            if (alive)
            {
                _thr.PostAction(
                    () => handle_alive_targets(callbackUri));

                _counters.Hosting_Event_AliveHosts++;
            }
            else
            {
                _thr.PostAction(
                    () => handle_dead_targets(callbackUri));

                _counters.Hosting_Event_DeadHosts++;
            }
        }

        public string ListeningUri
        {
            get { return _listeningUri; }
        }

        #endregion

        #region IHostingPeer

        void IHostingPeer.SubscribeTargets(TargetsInfo info)
            // one way message; arbitrary WCF thread
        {
            _thr.PostAction(
                () => subscribe_targets(info), "Sibscribing targets.{0} peer at '{1}'", info.Tag, info.ListeningUri);
        }

        void IHostingPeer.CancelTargets(string targetsTag)
            // one way message; arbitrary WCF thread
        {
            _thr.PostAction(
                () => cancel_targets(targetsTag), "Canceling targets.{0}", targetsTag);
        }

        #endregion

        #region protected api

        protected override void Stop(TimeSpan timeout)
        {
            // notify all targets

            _wcf.Stop(timeout);
            _thr.Stop();

            trace("hosting is stopped");
        }

        #endregion

        #region IVhosting

        void IVhosting.AddTarget(Action target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<R>(Func<R> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2>(string targetName, Action<T1, T2> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        R IVhosting.AddTarget<T1, T2, R>(string targetName, Func<T1, T2, R> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2>(Action<T1, T2> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<T1, R>(Func<T1, R> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2, R>(Func<T1, T2, R> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.Close()
        {
            _core.CloseHosting(this);
        }

        string IVhosting.ListeningUri
        {
            get { return _listeningUri; }
        }

        #endregion
    }
}