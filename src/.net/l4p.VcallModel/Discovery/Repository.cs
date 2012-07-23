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
        void Add(Subscriber subscriber);

        void Remove(Publisher publisher);
        void Remove(Subscriber subscriber);

        Publisher[] GetPublishers();
        Subscriber[] GetSubscribers();
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<Repository>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private List<Publisher> _publishers;
        private List<Subscriber> _subscribers;

        #endregion

        #region construction

        public Repository()
        {
            _publishers = new List<Publisher>();
            _subscribers = new List<Subscriber>();
        }

        #endregion

        #region IRepository

        void IRepository.Add(Publisher publisher)
        {
            var publishers = new List<Publisher>(_publishers);
            publishers.Add(publisher);

            _publishers = publishers;
        }

        void IRepository.Add(Subscriber subscriber)
        {
            var subscribers = new List<Subscriber>(_subscribers);
            subscribers.Add(subscriber);

            _subscribers = subscribers;
        }

        public void Remove(Publisher subject)
        {
            var publishers =
                from publisher in _publishers
                where !ReferenceEquals(publisher.Subject, subject)
                select publisher;

            _publishers = publishers.ToList();
        }

        public void Remove(Subscriber subject)
        {
            var subscribers =
                from subscriber in _subscribers
                where !ReferenceEquals(subscriber.Subject, subject)
                select subscriber;

            _subscribers = subscribers.ToList();
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

        #endregion
    }
}