/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.ServiceModel.Discovery;
using l4p.VcallModel.Core;
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

        // user arbitrary threads

        void PublishHostingPeer(string callbackUri, ICommNode node);
        void SubscribeTargetPeer(HostPeerNotification notify, ICommNode node);

        void CancelPublishedHosting(ICommNode node);
        void CancelSubscribedTarget(ICommNode node);

        DebugCounters DebugCounters { get; }

        // WCF arbitrary threads

        void HandleHelloMessage(EndpointDiscoveryMetadata edm);

        // resolver thread

        void SendHelloMessages();
        void GenerateByeNotifications(DateTime now);
    }

    class HostResolver : IHostResolver
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostResolver>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private readonly VcallConfiguration _config;
        private readonly Uri _resolvingScope;

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

            _resolvingScope = make_resolving_scope(config);
            trace("Resolving scope URI is '{0}'", _resolvingScope.ToString());

            _mutex = new Object();
            _repo = new Repository();
            _counters = new DebugCounters();

            _thread = new ResolvingThread(this, config);
            _discovery = new WcfDiscovery(this, config);
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("{0}: {1}", _config.ResolvingKey, msg);
        }

        private static Uri make_resolving_scope(VcallConfiguration config)
        {
            string resolvingScope = String.Format(config.DiscoveryScopePattern, config.ResolvingKey.ToLowerInvariant());

            return Helpers.TryCatch(
                () => new Uri(resolvingScope),
                ex => Helpers.ThrowNew<HostResolverException>(ex, "resolvingKye '{0}' is invalid; resulting uri is '{1}'", config.ResolvingKey, resolvingScope));
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

        void IHostResolver.PublishHostingPeer(string callbackUri, ICommNode node)
        {
            var uri = Helpers.TryCatch(
                () => new Uri(callbackUri),
                ex => Helpers.ThrowNew<HostResolverException>(ex, "Failed to parse callbackUri '{0}'", callbackUri));

            var publisher = new Publisher
            {
                ResolvingKey = _config.ResolvingKey,
                CallbackUri = uri,
                Node = node,
                ResolvingScope = _resolvingScope
            };

            lock (_mutex)
            {
                _repo.Add(publisher);
            }

            trace("host.{0} is published at '{1}'", node.Tag, callbackUri);
        }

        void IHostResolver.SubscribeTargetPeer(HostPeerNotification notify, ICommNode node)
        {
            var subscriber = new Subscriber
            {
                ResolvingKey = _config.ResolvingKey,
                Notify = notify,
                Node = node,
                ResolvingScope = _resolvingScope,
            };

            lock (_mutex)
            {
                _repo.Add(subscriber);
            }

            trace("target.{0} is subscribed", node.Tag);
        }

        void IHostResolver.CancelPublishedHosting(ICommNode node)
        {
            Publisher publisher;

            lock (_mutex)
            {
                publisher = _repo.RemovePublisher(node);
            }

            if (publisher == null)
            {
                _log.Warn("host.{0} is not found", node.Tag);
                return;
            }

            trace("host.{0} is canceled", node.Tag);
        }

        void IHostResolver.CancelSubscribedTarget(ICommNode node)
        {
            Subscriber subscriber;

            lock (_mutex)
            {
                subscriber = _repo.RemoveSubscriber(node);
            }

            if (subscriber == null)
            {
                _log.Warn("target.{0} is not found", node.Tag);
                return;
            }

            trace("target.{0} is canceled", node.Tag);
        }

        DebugCounters IHostResolver.DebugCounters
        {
            get
            {
                var counters = new DebugCounters();
                counters.Accumulate(_counters);

                return counters;
            }
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

                if (resolvingScope != _resolvingScope)
                {
                    _counters.HelloMsgsFiltered++;
                    _counters.OtherHelloMsgsReceived++;
                    return;
                }

                var subscribers = _repo.GetSubscribers();

                // collect to-be-done notifications
                foreach (var subscriber in subscribers)
                {
                    subscriber.GotAliveCallback(callbackUri, DateTime.Now, notifications);
                }

                _counters.MyHelloMsgsReceived++;
                _counters.HelloNotificationsProduced += notifications.Count;
            }

            // execute collected to-be-done notifications
            notifications.ForEach(notify => notify.Invoke());

            if (notifications.Count > 0)
            {
                trace("{0} hello notifications are generated", notifications.Count);
            }
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

            if (publishers.Length > 0)
            {
                trace("{0} hello messages are sent", publishers.Length);
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

            if (notifications.Count > 0)
            {
                trace("{0} bye notifications are generated", notifications.Count);
            }
        }

        #endregion
    }
}