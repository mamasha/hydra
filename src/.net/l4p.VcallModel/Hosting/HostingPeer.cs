/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Core;
using l4p.VcallModel.Target;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Hosting
{
    class HostingPeer 
        : CommNode
        , IHostingPeer, IVhosting
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostingPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly HostingConfiguration _config;
        private readonly IVcallSubsystem _core;

        private readonly Object _mutex;
        private readonly IRepository _repo;
        private readonly IWcfHostring _wcf;
        private readonly IHostingThread _thread;

        private string _listeningUri;

        #endregion

        #region construction

        public HostingPeer(HostingConfiguration config, VcallSubsystem core)
        {
            _config = config;
            _core = core;
            _mutex = new Object();
            _repo = Repository.New(this);
            _wcf = new WcfHosting(this);
            _thread = HostringThread.New(_tag);
        }

        #endregion

        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("hosting.{0}: {1}", _tag, msg);
        }

        private void update_targets_peer(TargetsInfo info)
            // hosting thread
        {
        }

        private void connect_to_targets(TargetsInfo info)
            // hosting thread
        {
            var myInfo = new HostingInfo
            {
                Tag = _tag,
                CallbackUri = _listeningUri,
                NameSpace = _config.NameSpace,
                HostName = "TBD"
            };

            info.Proxy.RegisterHosting(myInfo);
            _counters.Hosting_ConnectedTargets++;

            lock (_mutex)
            {
                _repo.AddTargets(info);
            }

            update_targets_peer(info);
        }

        private void subscribe_targets(TargetsInfo info)
            // hosting thread
        {
            trace("Got new targets.{0} at '{1}'", info.Tag, info.ListeningUri);

            info.Proxy = TargetsPeerProxy.New(info.ListeningUri);

            lock (_mutex)
            {
                _repo.AddTargets(info);
                _counters.Hosting_SubscribedTargets++;
            }

            connect_to_targets(info);
        }

        private void cancel_targets(string targetsTag)
            // hosting thread
        {
            trace("targets.{0} is canceled", targetsTag);
        }

        #region public api

        public void Start(string uri, TimeSpan timeout)
        {
            trace("starting at uri '{0}'", uri);

            _listeningUri = Helpers.TryCatch(_log,
                () => _wcf.Start(uri, timeout),
                ex => Helpers.ThrowNew<HostingPeerException>(ex, _log, "Failed to start hosting.{0} at '{1}'", _tag, uri));

            _thread.Start();

            trace("host is started");
        }

        public string Tag
        {
            get { return _tag; }
        }

        #endregion

        #region IHostingPeer

        void IHostingPeer.SubscribeTargets(TargetsInfo info)
            // one way message; arbitrary WCF thread
        {
            _thread.PostAction(
                () => subscribe_targets(info), "Sibscribing targets.{0} peer at '{1}'", info.Tag, info.ListeningUri);
        }

        void IHostingPeer.CancelTargets(string targetsTag)
            // one way message
        {
            _thread.PostAction(
                () => cancel_targets(targetsTag), "Canceling targets.{0}", targetsTag);
        }

        #endregion

        #region protected api

        protected override void Stop(TimeSpan timeout)
            // one way message; arbitrary WCF thread
        {

            _wcf.Stop(timeout);
            _thread.Stop();

            trace("hosting is stopped");
        }

        #endregion

        #region IVhosting

        void IVhosting.AddTarget(Action target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<R>(Func<R> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2>(string targetName, Action<T1, T2> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        R IVhosting.AddTarget<T1, T2, R>(string targetName, Func<T1, T2, R> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2>(Action<T1, T2> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<T1, R>(Func<T1, R> target)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2, R>(Func<T1, T2, R> target)
        {
            throw
                Helpers.NewNotImplementedException();
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