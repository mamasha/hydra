/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Core
{
    class HostingPeer 
        : CommNode
        , IHostingPeer, IVhosting
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostingPeer>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private readonly HostingConfiguration _config;
        private readonly IVcallSubsystem _core;

        private readonly string _listeningUri;

        #endregion

        #region construction

        public HostingPeer(HostingConfiguration config, VcallSubsystem core)
        {
            _config = config;
            _core = core;

            _listeningUri = "soap.tcp://localhost/" + Guid.NewGuid().ToString("B");
        }

        #endregion


        private void trace(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("host.{0}: {1}", _tag, msg);
        }
        #region public api

        public void Start()
        {
            _log.Trace("host.{0}: host is started", _tag);
        }

        public string ListeningUri
        {
            get { return _listeningUri; }
        }

        #endregion

        #region IHostingPeer

        void IHostingPeer.RegisterTargetPeer(string callbackUri)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region protected api

        protected override void Stop()
        {
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

        #endregion
    }
}