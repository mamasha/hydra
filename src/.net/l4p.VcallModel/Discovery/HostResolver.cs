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
    class HostResolver : IHostResolver
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostResolver>();
        private static readonly IHelpers Helpers = LoggedHelpers.New(_log);

        private readonly HostResolverConfiguration _config;
        private readonly Uri _resolvingScope;

        private readonly Object _mutex;
        private readonly IRepository _repo;
        private DebugCounters _counters;

        private readonly IWcfDiscovery _discovery;
        private readonly IResolvingThread _thread;

        #endregion

        #region construction

        public static IHostResolver New(HostResolverConfiguration config)
        {
            return
                new HostResolver(config);
        }

        private HostResolver(HostResolverConfiguration config)
        {
            _config = config;

            _resolvingScope = make_resolving_scope();
            trace("Resolving scope URI is '{0}'", _resolvingScope.ToString());

            _mutex = new Object();
            _repo = new Repository();
            _counters = new DebugCounters();

            _thread = new ResolvingThread(this, config.HelloMessageGap);
            _discovery = new WcfDiscovery(this, config);
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("{0}: {1}", _config.ResolvingKey, msg);
        }

        private Uri make_resolving_scope()
        {
            string resolvingScope = String.Format(_config.DiscoveryScopePattern, _config.ResolvingKey.ToLowerInvariant());

            return Helpers.TryCatch(
                () => new Uri(resolvingScope),
                ex => Helpers.ThrowNew<HostResolverException>(ex, "resolvingKye '{0}' is invalid; resulting uri is '{1}'", _config.ResolvingKey, resolvingScope));
        }

        private void send_hello_message_of(Publisher publisher)
        {
            var edm = new EndpointDiscoveryMetadata { Address = _discovery.Address };

            edm.ListenUris.Add(publisher.CallbackUri);
            edm.Scopes.Add(publisher.ResolvingScope);

            _discovery.SendHelloMessage(edm);

            lock (_mutex)
            {
                _counters.HelloMsgsSent++;
            }
        }

        private void update_subscriber(Subscriber subscriber)
        {
            throw new NotImplementedException();
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

            _thread.PostAction(() => send_hello_message_of(publisher));

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

            _thread.PostAction(() => update_subscriber(subscriber));

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

        void IHostResolver.HandleHelloMessage(EndpointDiscoveryMetadata edm, DateTime lastSeen)
        {
            Subscriber[] subscribers;
            string callbackUri;

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
                callbackUri = edm.ListenUris[0].ToString();

                if (resolvingScope != _resolvingScope)
                {
                    _counters.HelloMsgsFiltered++;
                    _counters.OtherHelloMsgsReceived++;
                    return;
                }

                _counters.MyHelloMsgsReceived++;

                subscribers = _repo.AddHelloMessage(callbackUri, lastSeen);

                if (subscribers == null)
                    return;

                _counters.HelloNotificationsProduced += subscribers.Length;
            }

            foreach (var subscriber in subscribers)
            {
                subscriber.Notify(callbackUri, true);
            }

            trace("{0} hello notifications are generated", subscribers.Length);
        }

        void IHostResolver.SendHelloMessages()
        {
            var publishers = _repo.GetPublishers();

            foreach (var publisher in publishers)
            {
                send_hello_message_of(publisher);
            }

            if (publishers.Length > 0)
            {
                trace("{0} hello messages are sent", publishers.Length);
            }
        }

        void IHostResolver.GenerateByeNotifications(DateTime now)
        {
            List<Action> notifications;

            lock (_mutex)
            {
                notifications = _repo.RemoveDeadHosts(now, _config.ByeMessageGap);
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