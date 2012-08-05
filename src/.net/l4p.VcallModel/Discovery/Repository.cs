/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Discovery
{
    class Publisher
    {
        public Uri CallbackUri { get; set; }
        public string Role { get; set; }
        public string Tag { get; set; }
    }

    class Subscriber
    {
        public PublishNotification OnPublish { get; set; }
        public string Tag { get; set; }
    }

    struct RemotePeer
    {
        public string CallbackUri { get; set; }
        public string Role { get; set; }
        public DateTime LastSeen { get; set; }
    }

    interface IRepository
    {
        void Add(Publisher publisher);
        void Remove(Publisher publisher);
        Publisher[] GetPublishers();

        void Add(Subscriber subscriber);
        void Remove(Subscriber subscriber);
        Subscriber[] GetSubscribers();

        void AddPeer(RemotePeer peer);
        bool HasPeer(string callbackUri);
        RemotePeer[] GetPeers();
        RemotePeer[] GetDeadPeers(DateTime lastSeen);
        void RemovePeers(RemotePeer[] peers);

        DebugCounters Counters { get; }
    }

    class Repository : IRepository
    {
        #region members

        private static readonly IHelpers Helpers = Helpers;

        private readonly List<Publisher> _publishers;
        private readonly List<Subscriber> _subscribers;
        private readonly Dictionary<string, RemotePeer> _peers;

        private readonly DebugCounters _counters;

        #endregion

        #region construction

        public Repository()
        {
            _publishers = new List<Publisher>();
            _subscribers = new List<Subscriber>();
            _peers = new Dictionary<string, RemotePeer>();
            _counters = new DebugCounters();
        }

        #endregion

        #region Implementation of IRepository

        void IRepository.Add(Publisher publisher)
        {
            _publishers.Add(publisher);
            _counters.ActivePublishers++;
        }

        void IRepository.Remove(Publisher publisher)
        {
            bool wasThere = _publishers.Remove(publisher);

            if (wasThere)
                _counters.ActivePublishers--;
        }

        Publisher[] IRepository.GetPublishers()
        {
            return
                _publishers.ToArray();
        }

        void IRepository.Add(Subscriber subscriber)
        {
            _subscribers.Add(subscriber);
            _counters.ActiveSubscribers++;
        }

        void IRepository.Remove(Subscriber subscriber)
        {
            bool wasThere = _subscribers.Remove(subscriber);

            if (wasThere)
                _counters.ActiveSubscribers--;
        }

        Subscriber[] IRepository.GetSubscribers()
        {
            return
                _subscribers.ToArray();
        }

        void IRepository.AddPeer(RemotePeer peer)
        {
            _peers[peer.CallbackUri] = peer;
            _counters.AliveRemotePeers++;
        }

        bool IRepository.HasPeer(string callbackUri)
        {
            return
                _peers.ContainsKey(callbackUri);
        }

        RemotePeer[] IRepository.GetPeers()
        {
            return
                _peers.Values.ToArray();
        }

        RemotePeer[] IRepository.GetDeadPeers(DateTime lastSeen)
        {
            var peers =
                from peer in _peers.Values
                where peer.LastSeen < lastSeen
                select peer;

            return
                peers.ToArray();
        }

        void IRepository.RemovePeers(RemotePeer[] peers)
        {
            foreach (var peer in peers)
            {
                bool wasThere = _peers.Remove(peer.CallbackUri);

                if (wasThere)
                    _counters.AliveRemotePeers--;
            }
        }

        DebugCounters IRepository.Counters
        {
            get { return _counters; }
        }

        #endregion

/*

        #region IRepository

        void IRepository.Add(Publisher publisher)
        {
            var publishers = _publishers;

            var newPublishers = new List<Publisher>(publishers);
            newPublishers.Add(publisher);

            _publishers = newPublishers;
        }

        string[] IRepository.Add(Subscriber subscriber)
        {
            var subscribers = _subscribers;
            var aliveHosts = _aliveHosts;

            var newSubscribers = new List<Subscriber>(subscribers);
            newSubscribers.Add(subscriber);

            _subscribers = newSubscribers;

            return 
                aliveHosts.Keys.ToArray();
        }

        void IRepository.Remove(Publisher publisher)
        {
            var publishers = _publishers;

            Publisher firstRef = publishers.Find(
                publisher => ReferenceEquals(publisher.Node, node));

            if (firstRef == null)
                return null;

            var newPublishers =
                from publisher in publishers
                where !ReferenceEquals(publisher.Node, node)
                select publisher;

            _publishers = newPublishers.ToList();

            return firstRef;
        }

        public Subscriber RemoveSubscriber(ICommNode node)
        {
            var subscribers = _subscribers;

            Subscriber firstRef = subscribers.Find(
                subscriber => ReferenceEquals(subscriber.Node, node));

            if (firstRef == null)
                return null;

            var newSubscribers =
                from subscriber in subscribers
                where !ReferenceEquals(subscriber.Node, node)
                select subscriber;

            _subscribers = newSubscribers.ToList();

            return firstRef;
        }

        Publisher[] IRepository.GetPublishers()
        {
            var publishers = _publishers;

            return
                publishers.ToArray();
        }

        Subscriber[] IRepository.GetSubscribers()
        {
            var subscribers = _subscribers;

            return
                subscribers.ToArray();
        }

        Subscriber[] IRepository.AddAliveHost(string callbackUri, DateTime lastSeen)
        {
            var subscribers = _subscribers;
            var aliveHosts = _aliveHosts;

            if (aliveHosts.ContainsKey(callbackUri))
            {
                aliveHosts[callbackUri] = lastSeen;
                return null;
            }

            var newAliveHosts = new Dictionary<string, DateTime>(aliveHosts);
            newAliveHosts[callbackUri] = lastSeen;

            _aliveHosts = aliveHosts;

            return 
                subscribers.ToArray();
        }

        string[] IRepository.GetAliveHosts()
        {
            var aliveHosts = _aliveHosts;

            return
                aliveHosts.Keys.ToArray();
        }

        List<Action> IRepository.RemoveDeadHosts(DateTime now, int alivePeriod)
        {
            var timeout = Helpers.TimeSpanFromMillis(alivePeriod);
            var notifications = new List<Action>();

            var subscribers = _subscribers;
            var aliveHosts = _aliveHosts;

            var newAliveHosts = new Dictionary<string, DateTime>();

            foreach (var pair in aliveHosts)
            {
                string callbackUri = pair.Key;
                DateTime lastSeen = pair.Value;

                if (now - lastSeen < timeout)
                {
                    newAliveHosts.Add(callbackUri, lastSeen);
                    continue;
                }

                foreach (var subscriber_ in subscribers)
                {
                    var subscriber = subscriber_;
                    notifications.Add(() => subscriber.Notify(callbackUri, false));
                }
            }

            _aliveHosts = newAliveHosts;
            return notifications;
        }

        #endregion
*/

    }
}