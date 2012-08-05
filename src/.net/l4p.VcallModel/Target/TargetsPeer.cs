/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
using System.ServiceModel;
using l4p.VcallModel.Core;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Target
{
    class TargetsPeer 
        : CommNode
        , ITargetsPeer, IVtarget
    {
        #region members

        private static readonly ILogger _log = Logger.New<TargetsPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly Object _mutex;
        private readonly TargetConfiguration _config;
        private readonly IVcallSubsystem _core;
        private readonly IRepository _repo;

        private readonly IWcfTarget _wcf;
        private string _listeningUri;

        #endregion

        #region construction

        public TargetsPeer(TargetConfiguration config, VcallSubsystem core)
        {
            _mutex = new Object();
            _config = config;
            _core = core;
            _repo = Repository.New();
            _wcf = new WcfTarget(this);
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("targets.{0}: {1}", _tag, msg);
        }

        private void handle_alive_hosting(string callbackUri)
        {
            _counters.Targets_AliveHosts++;

            var hosting = Helpers.TryCatch(_log,
                () => HostingPeerProxy.New(callbackUri),
                ex => Helpers.ThrowNew<TargetsPeerException>(ex, _log, "targets.{0}: Failed to create host at '{1}'", _tag, callbackUri));

            try
            {
                var myInfo = new TargetsInfo
                {
                    Tag = _tag,
                    ListeningUri = _listeningUri,
                    NameSpace = _config.NameSpace,
                    HostName = "TBD"
                };

                hosting.SubscribeTargets(myInfo);
                _counters.Targets_SubscribeTargets++;
            }
            catch (Exception ex)
            {
                if (ex.IsNotConsequenceOf<EndpointNotFoundException>())
                {
                    throw 
                        Helpers.MakeNew<TargetsPeerException>(ex, _log, "targets.{0}: Failed to register self at host '{1}'", _tag, callbackUri);
                }

                trace("new host at '{1}' is not responding; skipped; {0}", callbackUri, ex.GetDetailedMessage());

                return;
            }

            trace("subscribed to host '{0}' (hosts={1})", callbackUri, _repo.HostingsCount);
        }

        private void handle_dead_hosting(string callbackUri)
        {
            lock(_mutex)
            {
//                _repo.HostIdDead(callbackUri);
            }

            _counters.Targets_DeadHosts++;
//x            trace("disconnected from host '{0}' (hosts={1})", callbackUri, _repo.HostingsCount);
        }

        private void register_hosting(HostingInfo info)
        {
            throw new NotImplementedException();

            var hosting = Helpers.TryCatch(_log,
               () => HostingPeerProxy.New(info.CallbackUri),
               ex => Helpers.ThrowNew<TargetsPeerException>(ex, _log, "targets.{0}: Failed to create host at '{1}'", _tag, info.CallbackUri));

            lock (_mutex)
            {
                _repo.AddHosting(info);
            }
        }

        private void cancel_hosting(string hostingUri)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        #endregion

        #region public api

        public void Start(string uri, TimeSpan timeout)
        {
            trace("starting at uri '{0}'", uri);

            _listeningUri = Helpers.TryCatch(_log,
                () => _wcf.Start(uri, timeout),
                ex => Helpers.ThrowNew<HostingPeerException>(ex, _log, "Failed to start targets.{0} at '{1}'", _tag, uri));

            trace("target is started");
        }

        public string Tag
        {
            get { return _tag; }
        }

        // [discovery thread]
        public void OnHostingDiscovery(string callbackUri, string role, bool alive)
        {
            throw new NotImplementedException();

            if (alive)
                handle_alive_hosting(callbackUri);
            else
                handle_dead_hosting(callbackUri);
        }

        #endregion

        #region protected api

        protected override void Stop(TimeSpan timeout)
        {
            trace("target is stopped");

            // notify all targets

            // close all targets

            throw
                Helpers.NewNotImplementedException();
        }

        #endregion

        #region ITargetsPeer

        void ITargetsPeer.RegisterHosting(HostingInfo info)
        {
            // one way message

            try
            {
                register_hosting(info);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "targets.{0}: Failed to register hosting at '{1}'", _tag, info.CallbackUri);
            }
        }

        void ITargetsPeer.CancelHosting(string hostingTag)
        {
            // one way message

            try
            {
                cancel_hosting(hostingTag);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "targets.{0}: Failed to cancel hosting.{1}", _tag, hostingTag);
            }
        }

        #endregion

        #region IVtarget

        void IVtarget.Call(Expression<Action> vcall)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        R IVtarget.Call<R>(Expression<Func<R>> vcall)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVtarget.Call(string methodName, params object[] args)
        {
            throw
                Helpers.MakeNew<VcallException>(null, _log, "'{0}' There is no registered targets for subject '{1}'", "resolving key", methodName);

            throw
                Helpers.NewNotImplementedException();
        }

        R IVtarget.Call<R>(string functionName, params object[] args)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVtarget.Close()
        {
            _core.CloseTarget(this);
        }

        #endregion
    }
}