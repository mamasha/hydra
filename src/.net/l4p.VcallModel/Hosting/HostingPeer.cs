/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Core;
using l4p.VcallModel.Helpers;
using l4p.VcallModel.Target;

namespace l4p.VcallModel.Hosting
{
    class HostingPeer 
        : CommNode
        , IHostingPeer, IVhosting
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostingPeer>();
        private static readonly IHelpers Helpers = LoggedHelpers.New(_log);

        private readonly HostingConfiguration _config;
        private readonly IVcallSubsystem _core;

        private readonly Object _mutex;
        private readonly IRepository _repo;
        private readonly IWcfHostring _wcf;

        private string _listeningUri;

        #endregion

        #region construction

        public HostingPeer(HostingConfiguration config, VcallSubsystem core)
        {
            _config = config;
            _core = core;
            _mutex = new Object();
            _repo = Repository.New();
            _wcf = new WcfHosting(this);
        }

        #endregion

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("host.{0}: {1}", _tag, msg);
        }

        private void handle_new_target(string targetTag, string callbackUri)
        {
            var targetPeer = Helpers.TryCatch(
                () => TargetPeerProxy.New(callbackUri),
                ex => Helpers.ThrowNew<HostingPeerException>(ex, "host.{0}: Failed to connect to target.{1} peer at '{2}'", _tag, targetTag, callbackUri));

            // update target with all subjects

            lock (_mutex)
            {
                _repo.AddTarget(targetTag, targetPeer);
                _counters.Hosting_ConnectedTargets++;
            }
        }

        private void handle_dead_target(string targetTag)
        {
            throw new NotImplementedException();
        }

        #region public api

        public void Start(string uri, TimeSpan timeout)
        {
            trace("starting at uri '{0}'", uri);

            _listeningUri = Helpers.TryCatch(
                () => _wcf.Start(uri, timeout),
                ex => Helpers.ThrowNew<HostingPeerException>(ex, "Failed to start host.{0} at '{1}'", _tag, uri));


            trace("host is started");
        }

        public string Tag
        {
            get { return _tag; }
        }

        #endregion

        #region IHostingPeer

        void IHostingPeer.RegisterTargetPeer(string targetTag, string callbackUri)
        {
            trace("Got new target.{0} at '{1}'", targetTag, callbackUri);

            try
            {
                handle_new_target(targetTag, callbackUri);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to connect to target.{0} peer at '{1}'", targetTag, callbackUri);
            }
        }

        void IHostingPeer.UnregisterTargetPeer(string targetTag)
        {
            trace("target.{0} is dead", targetTag);
            handle_dead_target(targetTag);
        }

        #endregion

        #region protected api

        protected override void Stop(TimeSpan timeout)
        {
            // notify all targets

            _wcf.Stop(timeout);
            _log.Trace("host.{0}: hosting is stopped", _tag);
        }

        #endregion

        #region IVhosting

        void IVhosting.AddTarget(Action target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.AddTarget<R>(Func<R> target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2>(string targetName, Action<T1, T2> target)
        {
            throw new NotImplementedException();
        }

        R IVhosting.AddTarget<T1, T2, R>(string targetName, Func<T1, T2, R> target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2>(Action<T1, T2> target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.AddTarget<T1, R>(Func<T1, R> target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2, R>(Func<T1, T2, R> target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.Close()
        {
            _core.CloseHosting(this);
        }

        string IVhosting.ListeningUri
        {
            get { return _listeningUri; }
        }

        #endregion
    }
}