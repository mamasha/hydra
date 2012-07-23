/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.ServiceModel.Discovery;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Discovery
{
    delegate void HostPeerNotification(string callbackUri, bool alive);

    class HostResolverException : Exception
    {
        public HostResolverException() { }
        public HostResolverException(string message) : base(message) { }
        public HostResolverException(string message, Exception inner) : base(message, inner) { }
    }

    interface IHostResolver
    {
        void Start();
        void Stop();

        DebugCounters Counters { get; }

        // user arbitrary thread

        void PublishHostingPeer(string resolvingKey, string callbackUri, IDisposable publisher);
        void SubscribeTargetPeer(string resolvingKey, HostPeerNotification notify, IDisposable subscriber);
        void CancelPublishedHosting(Publisher publisher);
        void CancelSubscribedTarget(Subscriber subscriber);

        // worker thread

        void SendHelloMessages();
        void GenerateByeNotifications(DateTime now);

        // WCF arbitrary thread

        void HandleHelloMessage(EndpointDiscoveryMetadata edm);
    }

    class HostResolver : IHostResolver
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostResolver>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private readonly VcallConfiguration _config;

        private readonly Object _mutex;
        private readonly IRepository _repo;
        private DebugCounters _counters;

        private readonly IWcfDiscovery _discovery;
        private readonly IResolvingThread _thread;

        #endregion

        #region construction

        public static IHostResolver New(VcallConfiguration config)
        {
            return
                new HostResolver(config);
        }

        private HostResolver(VcallConfiguration config)
        {
            _config = config;

            _mutex = new Object();
            _repo = new Repository();
            _counters = new DebugCounters();

            _thread = new ResolvingThread(this, config);
            _discovery = new WcfDiscovery(this, config);
        }

        #endregion

        #region private

        private Uri make_resolving_scope(string resolvingKey)
        {
            string resolvingScope = String.Format(_config.DiscoveryScopePattern, resolvingKey.ToLowerInvariant());

            return Helpers.TryCatch(
                () => new Uri(resolvingScope),
                ex => Helpers.ThrowNew<HostResolverException>(ex, "resolvingKye '{0}' is invalid; resulting uri is '{1}'", resolvingKey, resolvingScope));
        }

        #endregion

        #region IHostResolver

        void IHostResolver.Start()
        {
            _discovery.Start();
            _thread.Start();

            _log.Info("Host resolving service is started");
        }

        void IHostResolver.Stop()
        {
            _thread.Stop();
            _discovery.Stop();

            _log.Info("Host resolving service is stopped");
        }

        DebugCounters IHostResolver.Counters
        {
            get { return _counters; }
        }

        void IHostResolver.PublishHostingPeer(string resolvingKey, string callbackUri, IDisposable subject)
        {
            var uri = Helpers.TryCatch(
                () => new Uri(callbackUri),
                ex => Helpers.ThrowNew<HostResolverException>(ex, "Failed to parse callbackUri '{0}'", callbackUri));

            Uri resolvingScope = make_resolving_scope(resolvingKey);

            var publisher = new Publisher
            {
                ResolvingKey = resolvingKey,
                CallbackUri = uri,
                Subject = subject,
                ResolvingScope = resolvingScope
            };

            lock (_mutex)
            {
                _repo.Add(publisher);
            }

            _log.Trace("[{0}] publisher at '{1}' is added", resolvingKey, callbackUri);
        }

        void IHostResolver.SubscribeTargetPeer(string resolvingKey, HostPeerNotification notify, IDisposable subject)
        {
            Uri resolvingScope = make_resolving_scope(resolvingKey);

            var subscriber = new Subscriber
            {
                ResolvingKey = resolvingKey,
                Notify = notify,
                Subject = subject,
                ResolvingScope = resolvingScope,
            };

            lock (_mutex)
            {
                _repo.Add(subscriber);
            }

            _log.Trace("[{0}] a listener is added", resolvingKey);
        }

        void IHostResolver.CancelPublishedHosting(Publisher publisher)
        {
            lock (_mutex)
            {
                _repo.Remove(publisher);
            }

            _log.Trace("[{0}] published calbackUri '{1}' is canceled", publisher.ResolvingKey, publisher.CallbackUri);
        }

        void IHostResolver.CancelSubscribedTarget(Subscriber subscriber)
        {
            lock (_mutex)
            {
                _repo.Remove(subscriber);
            }

            _log.Trace("[{0}] subscribed target is canceled", subscriber.ResolvingKey);
        }

        void IHostResolver.SendHelloMessages()
        {
            var publishers = _repo.GetPublishers();

            foreach (var publisher in publishers)
            {
                var edm = new EndpointDiscoveryMetadata { Address = _discovery.Address };

                edm.ListenUris.Add(publisher.CallbackUri);
                edm.Scopes.Add(publisher.ResolvingScope);

                _discovery.SendHelloMessage(edm);
                _counters.HelloMsgsSent++;
            }
        }

        void IHostResolver.GenerateByeNotifications(DateTime now)
        {
            var aliveSpan = Helpers.MakeTimeSpan(_config.Timeouts.ServiceAliveTimeSpan);
            var subscribers = _repo.GetSubscribers();
            var notifications = new List<Action>();

            lock (_mutex)
            {
                foreach (var subscriber in subscribers)
                {
                    subscriber.RemoveDeadCallbacks(now, aliveSpan, notifications);
                }

                _counters.ByeNotificationsProduced += notifications.Count;
            }

            notifications.ForEach(notify => notify.Invoke());
        }

        void IHostResolver.HandleHelloMessage(EndpointDiscoveryMetadata edm)
        {
            var notifications = new List<Action>();

            lock (_mutex)
            {
                _counters.HelloMsgsRecieved++;

                if (edm.Scopes.Count != 1)
                {
                    _counters.HelloMsgsFiltered++;
                    return;
                }

                if (edm.ListenUris.Count != 1)
                {
                    _counters.HelloMsgsFiltered++;
                    return;
                }

                Uri resolvingScope = edm.Scopes[0];
                string callbackUri = edm.ListenUris[0].ToString();

                var subscribers = _repo.GetSubscribers();

                foreach (var subscriber in subscribers)
                {
                    if (subscriber.ResolvingScope != resolvingScope)
                    {
                        _counters.OtherHelloMsgsReceived++;
                        continue;
                    }

                    _counters.MyHelloMsgsReceived++;
                    subscriber.GotAliveCallback(callbackUri, DateTime.Now, notifications);
                }

                _counters.HelloNotificationsProduced += notifications.Count;
            }

            notifications.ForEach(notify => notify.Invoke());
        }

        #endregion
    }
}