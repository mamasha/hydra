/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Diagnostics;
using l4p.VcallModel.Core;
using l4p.VcallModel.ProxyPeers;
using l4p.VcallModel.Utils;
using l4p.VcallModel.VcallSubsystems;

namespace l4p.VcallModel.HostingPeers
{
    class HostingPeer 
        : CommPeer, IHostingPeer
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
            _repo = Repository.New();
            _wcf = new WcfHosting(this);

            var thrConfig = new ActiveThread.Config
            {
                Name = String.Format("hosting.{0}", _tag),
                DurableTimeToLive = core.Config.Timeouts.ActiveThread_DurableTimeToLive,
                StartTimeout = core.Config.Timeouts.ActiveThread_Start,
                StopTimeout = core.Config.Timeouts.ActiveThread_Stop,
                RunInContextOf = main => _core.RunInMyContext(main)
            };

            _thr = ActiveThread.New(thrConfig);
        }

        #endregion

        private void trace(string format, params object[] args)
        {
            if (_log.TraceIsOff)
                return;

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

        private void handle_proxy_hello(string callbackUri)
        {
            trace("hello from proxy.{0}", callbackUri);

            var myInfo = new HostingInfo
            {
                Tag = _tag,
                CallbackUri = _listeningUri,
                NameSpace = _config.NameSpace,
                HostName = Helpers.GetLocalhostFqdn()
            };

            _thr.DoOnce(_config.SubscribeToProxy_RetryTimeout.Value, callbackUri,
                () => subsribe_self_to_proxy(callbackUri, myInfo), "subscribe hosting.{0} to {1}", _tag, callbackUri);
        }

        private void handle_proxy_bye(string callbackUri)
        {
            trace("bye from proxy.{0}", callbackUri);

            _thr.Cancel(callbackUri);

            var info = _repo.FindProxy(callbackUri);

            if (info == null)
                return;

            _repo.RemoveProxy(info);

            trace("proxy at '{0}' is dead (proxies={1})", callbackUri, _repo.ProxyCount);
        }

        private void subsribe_self_to_proxy(string callbackUri, HostingInfo myInfo)
        {
            var proxy = Channels.ProxyPeer.New(callbackUri);

            try_catch(
                () => proxy.SubscribeHosting(myInfo),
                () => ++_counters.Hosting_Error_SubscribeToProxy, "hosting.{0}: Failed to subscribe self at host '{1}'", _tag, callbackUri);

            _counters.Hosting_Event_SubscribedToProxy++;
            trace("subscribed to proxy at '{0}' (proxies={1})", callbackUri, _repo.ProxyCount);
        }

        private void update_proxy_peer(ProxyInfo info)
        {
            // generate update messages
            int count = 0;

            trace("{0} update messages are generated", count);
        }

        private void subscribe_proxy(ProxyInfo info)
        {
            trace("Got a new proxy.{0} (namespace='{1}') at '{2}'", info.Tag, info.NameSpace, info.ListeningUri);

            if (info.NameSpace != _config.NameSpace)
            {
                trace("Not a my namespace proxy.{0} (namespace='{1}'); mine='{2}'", info.Tag, info.NameSpace, _config.NameSpace);
                _counters.Hosting_Event_NotMyNamespace++;
                return;
            }

            if (_repo.HasProxy(info.Tag))
            {
                trace("proxy.{0} at {1} is already registered", info.Tag, info.ListeningUri);
                _counters.Hosting_Event_KnownProxy++;
                return;
            }

            info.Proxy = Channels.ProxyPeer.New(info.ListeningUri);

            _repo.AddProxy(info);
            _counters.Hosting_Event_NewProxy++;

            update_proxy_peer(info);
        }

        private void cancel_proxy(string proxyTag)
        {
            var info = _repo.FindProxy(proxyTag);

            if (info == null)
            {
                trace("unknown proxy.{0}", proxyTag);
                _counters.Hosting_Event_UnknownProxy++;
                return;
            }

            _repo.RemoveProxy(info);
            _counters.Hosting_Event_CanceledProxy++;

            trace("proxy.{0} is canceled", proxyTag);
        }

        private void stop(TimeSpan timeout, IDoneEvent observer)
        {
            trace("stopping...");
            var timer = Stopwatch.StartNew();

            if (_state == State.Stopped)
            {
                _counters.Hosting_Event_IsAlreadyStopped++;
                return;
            }

            _state = State.Stopped;

            var proxies = _repo.GetProxies();

            foreach (var proxy in proxies)
            {
                _thr.Cancel(proxy.Tag);
                _repo.RemoveProxy(proxy);

                try
                {
                    proxy.Proxy.CancelHosting(_tag);
                }
                catch (Exception ex)
                {
                    warn("Failed to cancel proxy.{0} gracefully; {1}", proxy.Tag, ex.GetDetailedMessage());
                }
            }

            trace("stopping... all proxies are notified (int {0} msecs)", timer.ElapsedMilliseconds);

            _thr.CancelAll();

            _wcf.Stop(timeout);
            trace("stopping... wcf peers are stopped (in {0} msecs)", timer.ElapsedMilliseconds);

            _thr.SetStopSignal();
            _counters.Hosting_Event_IsStopped++;

            trace("hosting is stopped (in {0} msecs)", timer.ElapsedMilliseconds);

            observer.Signal();
        }

        private void start_hosting_of(Action action)
        {
            
        }

        #region public api

        public void Start(string uri, TimeSpan timeout)
        {
            trace("starting at uri '{0}'", uri);

            _listeningUri = Helpers.TryCatch(_log,
                () => _wcf.Start(uri, timeout),
                ex => Helpers.ThrowNew<HostingPeerException>(ex, _log, "Failed to start hosting.{0} at '{1}'", _tag, uri));

            _thr.Start();

            _state = State.Started;
            _counters.Hosting_Event_IsStarted++;
        }

        public string Tag
        {
            get { return _tag; }
        }

        public void HandlePublisher(string callbackUri, string role, bool alive)
            // discovery thread
        {
            if (role != _config.ProxyRole)
                return;

            if (alive)
            {
                _thr.PostAction(
                    () => handle_proxy_hello(callbackUri), "handling hello from {0}.{1}", role, callbackUri);

                _counters.Hosting_Event_HelloFromProxy++;
            }
            else
            {
                _thr.PostAction(
                    () => handle_proxy_bye(callbackUri), "handling bye from {0}.{1}", role, callbackUri);

                _counters.Hosting_Event_ByeFromProxy++;
            }
        }

        public string ListeningUri
        {
            get { return _listeningUri; }
        }

        #endregion

        #region IHostingPeer

        void IHostingPeer.SubscribeProxy(ProxyInfo info)
            // one way message; arbitrary WCF thread
        {
            _thr.PostAction(
                () => subscribe_proxy(info), "Sibscribing proxy.{0} peer at '{1}'", info.Tag, info.ListeningUri);
        }

        void IHostingPeer.CancelProxy(string proxyTag)
            // one way message; arbitrary WCF thread
        {
            _thr.PostAction(
                () => cancel_proxy(proxyTag), "Canceling proxy.{0}", proxyTag);
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
    }
}