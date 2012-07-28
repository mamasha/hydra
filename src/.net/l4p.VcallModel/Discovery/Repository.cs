/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Discovery
{
    interface IRepository
    {
        void Add(Publisher publisher);
        string[] Add(Subscriber subscriber);

        Publisher RemovePublisher(ICommNode node);
        Subscriber RemoveSubscriber(ICommNode node);

        Publisher[] GetPublishers();
        Subscriber[] GetSubscribers();

        Subscriber[] AddHelloMessage(string callbackUri, DateTime lastSeen);
        List<Action> RemoveDeadHosts(DateTime now, int alivePeriod);
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<Repository>();
        private static readonly IHelpers Helpers = LoggedHelpers.New(_log);

        private List<Publisher> _publishers;
        private List<Subscriber> _subscribers;
        private Dictionary<string, DateTime> _aliveHosts;

        #endregion

        #region construction

        public Repository()
        {
            _publishers = new List<Publisher>();
            _subscribers = new List<Subscriber>();
            _aliveHosts = new Dictionary<string, DateTime>();
        }

        #endregion

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

        public Publisher RemovePublisher(ICommNode node)
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

        Subscriber[] IRepository.AddHelloMessage(string callbackUri, DateTime lastSeen)
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
    }
}