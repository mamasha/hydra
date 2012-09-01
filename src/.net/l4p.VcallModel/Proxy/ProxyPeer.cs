/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using l4p.VcallModel.Core;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Manager;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Proxy
{
    class ProxyPeer 
        : CommNode
        , IProxyPeer, IProxy
    {
        #region members

        private static readonly ILogger _log = Logger.New<ProxyPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly ProxyConfiguration _config;
        private readonly IVcallSubsystem _core;
        private readonly IRepository _repo;

        private readonly IWcfProxy _wcf;
        private readonly IActiveThread _thr;

        private string _listeningUri;

        #endregion

        #region construction

        public ProxyPeer(ProxyConfiguration config, VcallSubsystem core)
        {
            _config = config;
            _core = core;
            _repo = Repository.New();
            _wcf = new WcfProxy(this);

            var thrConfig = new ActiveThread.Config
            {
                Name = String.Format("proxy.{0}", _tag),
                DurableTimeToLive = core.Config.Timeouts.ActiveThread_DurableTimeToLive,
                StartTimeout = core.Config.Timeouts.ActiveThread_Start,
                StopTimeout = core.Config.Timeouts.ActiveThread_Stop,
                RunInContextOf = main => _core.RunInMyContext(main)
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
            _log.Trace("proxy.{0}: {1}", _tag, msg);
        }

        private void warn(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Warn("proxy.{0}: {1}", _tag, msg);
        }

        private void try_catch(Action action, Action onError, string errFmt, params object[] args)
        {
            try { action(); }
            catch (Exception ex)
            {
                onError();
                throw Helpers.MakeNew<ProxyPeerException>(ex, _log, errFmt, args);
            }
        }

        private R try_catch<R>(Func<R> func, Action onError, string errFmt, params object[] args)
        {
            try { return func(); }
            catch (Exception ex)
            {
                onError();
                throw Helpers.MakeNew<ProxyPeerException>(ex, _log, errFmt, args);
            }
        }

        private void subsribe_self_to_hosting(string callbackUri, ProxyInfo myInfo)
        {
            var hosting = Channels.HostingPeer.New(callbackUri);

            try_catch(
                () => hosting.SubscribeProxy(myInfo),
                () => ++_counters.Proxy_Error_SubscribeToHosting, "proxy.{0}: Failed to register self at host '{1}'", _tag, callbackUri);

            _counters.Proxy_Event_SubscribedToHosting++;
            trace("subscribed to host '{0}' (hosts={1})", callbackUri, _repo.HostingCount);
        }

        private void handle_hosting_hello(string callbackUri)
        {
            trace("hello from hosting.{0}", callbackUri);

            var myInfo = new ProxyInfo
            {
                Tag = _tag,
                ListeningUri = _listeningUri,
                NameSpace = _config.NameSpace,
                HostName = Helpers.GetLocalhostFqdn()
            };

            _thr.DoOnce(_config.SubscribeToHosting_RetryTimeout.Value, callbackUri,
                () => subsribe_self_to_hosting(callbackUri, myInfo), "subscribe proxy.{0} to {1}", _tag, callbackUri);
        }

        private void handle_hosting_bye(string callbackUri)
        {
            trace("bye from hosting.{0}", callbackUri);

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
                _counters.Proxy_Event_KnownHosting++;
                return;
            }

            info.Proxy = Channels.HostingPeer.New(info.CallbackUri);

            _repo.AddHosting(info);
            _counters.Proxy_Event_NewHosting++;
        }

        private void cancel_hosting(string hostingTag)
        {
            var info = _repo.FindHosting(hostingTag);

            if (info == null)
            {
                trace("unknown hosting.{0}", hostingTag);
                _counters.Proxy_Event_UnknownHosting++;
                return;
            }

            _repo.RemoveHosting(info);
            _counters.Proxy_Event_CanceledHosting++;

            trace("hosting.{0} is canceled", hostingTag);
        }

        private void stop(TimeSpan timeout, IDoneEvent observer)
        {
            trace("stopping...");
            var timer = Stopwatch.StartNew();

            if (_state == State.Stopped)
            {
                _counters.Proxy_Event_IsAlreadyStopped++;
                return;
            }

            _state = State.Stopped;

            var hostings = _repo.GetHostings();

            foreach (var hosting in hostings)
            {
                _repo.RemoveHosting(hosting);
                _thr.Cancel(hosting.Tag);

                try
                {
                    hosting.Proxy.CancelProxy(_tag);
                }
                catch (Exception ex)
                {
                    warn("Failed to cancel hosting.{0} gracefully; {1}", hosting.Tag, ex.GetDetailedMessage());
                }
            }

            trace("stopping... all proxies are notified (in {0} msecs)", timer.ElapsedMilliseconds);

            _thr.CancelAll();

            _wcf.Stop(timeout);
            trace("stopping... wcf peers are stopped (in {0} msecs)", timer.ElapsedMilliseconds);

            _thr.SetStopSignal();
            _counters.Proxy_Event_IsStopped++;

            trace("proxy is stopped (in {0} msecs)", timer.ElapsedMilliseconds);

            observer.Signal();
        }

        #endregion

        #region public api

        public void Start(string uri, TimeSpan timeout)
        {
            trace("starting at uri '{0}'", uri);

            _listeningUri = Helpers.TryCatch(_log,
                () => _wcf.Start(uri, timeout),
                ex => Helpers.ThrowNew<HostingPeerException>(ex, _log, "Failed to start proxy.{0} at '{1}'", _tag, uri));

            _thr.Start();

            _state = State.Started;
            _counters.Proxy_Event_IsStarted++;
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
                    () => handle_hosting_hello(callbackUri), "handle hello from {0}.{1}", role, callbackUri);

                _counters.Proxy_Event_HelloFromHosting++;
            }
            else
            {
                _thr.PostAction(
                    () => handle_hosting_bye(callbackUri), "handle bye from {0}.{1}", role, callbackUri);

                _counters.Proxy_Event_ByeFromHosting++;
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

        #region IProxyPeer

        void IProxyPeer.SubscribeHosting(HostingInfo info)
            // one way message; arbitrary WCF thread
        {
            _thr.PostAction(
                () => subscribe_hosting(info), "Sibscribing hosting.{0} peer at '{1}'", info.Tag, info.CallbackUri);
        }

        void IProxyPeer.CancelHosting(string hostingTag)
            // one way message; arbitrary WCF thread
        {
            _thr.PostAction(
                () => cancel_hosting(hostingTag), "Canceling hosting.{0}", hostingTag);
        }

        #endregion

        #region IProxy

        void IProxy.Call(Expression<Action> vcall)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        R IProxy.Call<R>(Expression<Func<R>> vcall)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IProxy.Call(string methodName, params object[] args)
        {
            throw
                Helpers.MakeNew<VcallException>(null, _log, "'{0}' There is no registered proxies for subject '{1}'", "resolving key", methodName);

            throw
                Helpers.NewNotImplementedException();
        }

        R IProxy.Call<R>(string functionName, params object[] args)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        #endregion
    }
}