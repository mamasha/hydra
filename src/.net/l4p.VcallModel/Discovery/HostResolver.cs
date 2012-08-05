/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Discovery
{
    class HostResolver : IHostResolver
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostResolver>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly Self _self;
        private readonly IEngine _engine;

        #endregion

        #region construction

        public static IHostResolver New(HostResolverConfiguration config)
        {
            return
                new HostResolver(config);
        }

        private HostResolver(HostResolverConfiguration config)
        {
            _self = new Self();
            _engine = new Engine(_self);

            _self.config = config;

            _self.resolvingScope = make_resolving_scope(_self);
            trace("Resolving scope URI is '{0}'", _self.resolvingScope.ToString());

            _self.repo = new Repository();
            _self.thread = new ResolvingThread(_self, _engine);
            _self.wcf = new WcfDiscovery(_self, _engine);
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("{0}: {1}", _self.config.ResolvingKey, msg);
        }

        private DebugCounters get_accumulated_counters()
        {
            var counters = new DebugCounters();

            counters.Accumulate(_engine.Counters);
            counters.Accumulate(_self.repo.Counters);
            counters.Accumulate(_self.wcf.Counters);
            counters.Accumulate(_self.thread.Counters);

            return counters;
        }

        private static Uri make_resolving_scope(Self self)
        {
            string resolvingScope = String.Format(self.config.DiscoveryScopePattern, self.config.ResolvingKey.ToLowerInvariant());

            return Helpers.TryCatch(_log,
                () => new Uri(resolvingScope),
                ex => Helpers.ThrowNew<HostResolverException>(ex, _log, "resolvingKye '{0}' is invalid; resulting uri is '{1}'", self.config.ResolvingKey, resolvingScope));
        }

        private void publish(Publisher publisher)
        {
            _self.thread.PostAction(
                () => _engine.Publish(publisher));
        }

        private void unpublish(Publisher publisher)
        {
            _self.thread.PostAction(
                () => _engine.UnPublish(publisher));
        }

        private void subscribe(Subscriber subscriber)
        {
            _self.thread.PostAction(
                () => _engine.Subscribe(subscriber));
        }

        private void unsubscibe(Subscriber subscriber)
        {
            _self.thread.PostAction(
                () => _engine.UnSubscribe(subscriber));
        }

        #endregion

        #region IHostResolver

        void IHostResolver.Start()
        {
            _self.wcf.Start();
            _self.thread.Start();

            _log.Info("Host resolving service is started");
        }

        void IHostResolver.Stop()
        {
            _self.thread.Stop();
            _self.wcf.Stop();

            _log.Info("Host resolving service is stopped");
        }

        IRevertable IHostResolver.Publish(string callbackUri, string role, string tag)
        {
            var uri = Helpers.TryCatch(_log,
                () => new Uri(callbackUri),
                ex => Helpers.ThrowNew<HostResolverException>(ex, _log, "Failed to parse callbackUri '{0}'", callbackUri));

            var janitor = new Janitor();

            var publisher = new Publisher
                                {
                                    CallbackUri = uri,
                                    Role = role,
                                    Tag = tag
                                };

            publish(publisher);

            janitor.Add(
                () => unpublish(publisher));

            return janitor;
        }

        IRevertable IHostResolver.Subscribe(PublishNotification onPublish, string tag)
        {
            var janitor = new Janitor();

            var subscriber = new Subscriber
                                 {
                                     OnPublish = onPublish,
                                     Tag = tag
                                 };

            subscribe(subscriber);

            janitor.Add(
                () => unsubscibe(subscriber));

            return janitor;
        }

        DebugCounters IHostResolver.Counters
        {
            get { return get_accumulated_counters(); }
        }

        #endregion
    }
}