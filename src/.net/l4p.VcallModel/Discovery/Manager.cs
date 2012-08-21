using System;
using System.Collections.Generic;
using System.ServiceModel.Discovery;
using System.Xml;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Discovery
{
    interface IManager
    {
        void Publish(Publisher publisher);
        void Subscribe(Subscriber subscriber);
        void Cancel(string tag);

        void HandleHelloMessage(string callbackUri, string role, DateTime lastSeen);
        void SendHelloMessages();
        void GenerateByeNotifications(DateTime now);
    }

    class Manager : IManager
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostResolver>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly Self _self;

        private readonly DebugCounters _counters;

        #endregion

        #region construction

        public Manager(Self self)
        {
            _self = self;
            _counters = Context.Get<ICountersDb>().NewCounters();
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("discovery.{0}: {1}", _self.config.ResolvingKey, msg);
        }

        private void warn(Exception ex, string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Warn(ex, "discovery.{0}: {1}", _self.config.ResolvingKey, msg);
        }

        private void send_hello_message_of(Publisher publisher)
        {
            var edm = new EndpointDiscoveryMetadata { Address = _self.wcf.Address };

            edm.ListenUris.Add(publisher.CallbackUri);
            edm.Scopes.Add(_self.resolvingScope);
            edm.ContractTypeNames.Add(new XmlQualifiedName(publisher.Role));

            _self.wcf.SendHelloMessage(edm);
        }

        private void update_subscriber(Subscriber subscriber)
        {
            var peers = _self.repo.GetPeers();
            int count = peers.Length;

            foreach (var peer in peers)
            {
                fire_notification(subscriber, peer.CallbackUri, peer.Role, true);
            }

            _counters.Discovery_Event_HelloNotificationsProduced += count;
            trace("{0} hello notifications are generated", count);
        }

        private void fire_notification(Subscriber subscriber, string callbackUri, string role, bool alive)
        {
            try
            {
                subscriber.OnPublish(callbackUri, role, alive);
            }
            catch (Exception ex)
            {
                warn(ex, "Failed while notifying targets.{0}", subscriber.Tag);
            }
        }

        private void cancel_publishers(string tag)
        {
            var publishers = _self.repo.GetPublishers(tag);

            if (publishers.Length == 0)
                return;

            Array.ForEach(publishers, 
                publisher => _self.repo.Remove(publisher));

            trace("hosting.{0} is canceled (count={1})", tag, publishers.Length);
        }

        private void cancel_subscribers(string tag)
        {
            var subscribers = _self.repo.GetSubscribers(tag);

            if (subscribers.Length == 0)
                return;

            Array.ForEach(subscribers,
                subscriber => _self.repo.Remove(subscriber));

            trace("target.{0} is canceled (count={1})", tag, subscribers.Length);
        }

        #endregion

        #region IManager

        void IManager.Publish(Publisher publisher)
        {
            _self.repo.Add(publisher);

            send_hello_message_of(publisher);

            trace("{0}.{1} is published at '{2}'", publisher.Role, publisher.Tag, publisher.CallbackUri);
        }

        void IManager.Subscribe(Subscriber subscriber)
        {
            _self.repo.Add(subscriber);

            update_subscriber(subscriber);

            trace("targets.{0} is subscribed", subscriber.Tag);
        }

        void IManager.Cancel(string tag)
        {
            cancel_publishers(tag);
            cancel_subscribers(tag);
        }

        void IManager.HandleHelloMessage(string callbackUri, string role, DateTime lastSeen)
        {
            if (_self.repo.HasPeer(callbackUri))
            {
                _self.repo.KeepAlive(callbackUri, lastSeen);

                _counters.Discovery_Event_HelloMsgIsKeepAlive++;
                return;
            }

            var peer = new RemotePeer
                           {
                               CallbackUri = callbackUri,
                               Role = role,
                               LastSeen = lastSeen
                           };

            _self.repo.AddPeer(peer);

            var subscribers = _self.repo.GetSubscribers();
            int count = subscribers.Length;

            foreach (var subscriber in subscribers)
            {
                fire_notification(subscriber, callbackUri, role, true);
            }

            _counters.Discovery_Event_HelloNotificationsProduced += count;
            trace("{0} hello notifications are generated", count);
        }

        void IManager.SendHelloMessages()
        {
            var publishers = _self.repo.GetPublishers();

            foreach (var publisher in publishers)
            {
                send_hello_message_of(publisher);
            }

            if (publishers.Length > 0)
            {
                trace("{0} hello messages are sent", publishers.Length);
            }
        }

        void IManager.GenerateByeNotifications(DateTime now)
        {
            var lastSeen = now - TimeSpan.FromMilliseconds(_self.config.ByeMessageGap);

            var peers = _self.repo.GetDeadPeers(lastSeen);
            var subscribers = _self.repo.GetSubscribers();

            if (peers.Length == 0)
                return;

            var notificationCount = 0;

            foreach (var subsriber in subscribers)
            {
                foreach (var peer in peers)
                {
                    fire_notification(subsriber, peer.CallbackUri, peer.Role, false);
                    notificationCount++;
                }
            }

            _self.repo.RemovePeers(peers);
            _counters.Discovery_Event_ByeNotificationsProduced += notificationCount;

            if (notificationCount > 0)
                trace("{0} bye notifications are generated", notificationCount);
        }

        #endregion
    }
}