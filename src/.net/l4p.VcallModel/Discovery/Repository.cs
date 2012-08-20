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

    class RemotePeer
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
        Publisher[] GetPublishers(string tag);

        void Add(Subscriber subscriber);
        void Remove(Subscriber subscriber);
        Subscriber[] GetSubscribers();
        Subscriber[] GetSubscribers(string tag);

        void AddPeer(RemotePeer peer);
        bool HasPeer(string callbackUri);
        void KeepAlive(string callbackUri, DateTime lastSeen);
        RemotePeer[] GetPeers();
        RemotePeer[] GetDeadPeers(DateTime lastSeen);
        void RemovePeers(RemotePeer[] peers);
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<Repository>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

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
            _counters = Context.Get<ICountersDb>().NewCounters();
        }

        #endregion

        #region Implementation of IRepository

        void IRepository.Add(Publisher publisher)
        {
            _publishers.Add(publisher);
            _counters.Discovery_State_ActivePublishers++;
        }

        void IRepository.Remove(Publisher publisher)
        {
            bool wasThere = _publishers.Remove(publisher);

            if (wasThere)
                _counters.Discovery_State_ActivePublishers--;
        }

        Publisher[] IRepository.GetPublishers()
        {
            return
                _publishers.ToArray();
        }

        Publisher[] IRepository.GetPublishers(string tag)
        {
            var publishers =
                from publisher in _publishers
                where publisher.Tag == tag
                select publisher;

            return
                publishers.ToArray();
        }

        void IRepository.Add(Subscriber subscriber)
        {
            _subscribers.Add(subscriber);
            _counters.Discovery_State_ActiveSubscribers++;
        }

        void IRepository.Remove(Subscriber subscriber)
        {
            bool wasThere = _subscribers.Remove(subscriber);

            if (wasThere)
                _counters.Discovery_State_ActiveSubscribers--;
        }

        Subscriber[] IRepository.GetSubscribers()
        {
            return
                _subscribers.ToArray();
        }

        Subscriber[] IRepository.GetSubscribers(string tag)
        {
            var subsribers =
                from subscriber in _subscribers
                where subscriber.Tag == tag
                select subscriber;

            return
                subsribers.ToArray();
        }

        void IRepository.AddPeer(RemotePeer peer)
        {
            _peers[peer.CallbackUri] = peer;
            _counters.Discovery_State_AliveRemotePeers++;
        }

        bool IRepository.HasPeer(string callbackUri)
        {
            return
                _peers.ContainsKey(callbackUri);
        }

        void IRepository.KeepAlive(string callbackUri, DateTime lastSeen)
        {
            RemotePeer peer;

            _peers.TryGetValue(callbackUri, out peer);
            Helpers.Assert(peer != null, _log);

            peer.LastSeen = lastSeen;
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
                    _counters.Discovery_State_AliveRemotePeers--;
            }
        }

        #endregion
    }
}