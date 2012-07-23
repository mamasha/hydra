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
    class HostingPeer : IHostingPeer, IVhosting
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostingPeer>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private readonly HostingConfiguration _config;
        private readonly string _listeningUri;

        #endregion

        #region construction

        public HostingPeer(HostingConfiguration config)
        {
            _config = config;

            _listeningUri = "soap.tcp://localhost/" + Guid.NewGuid().ToString("B");
        }

        #endregion

        #region public api

        public string ListeningUri
        {
            get { return _listeningUri; }
        }

        #endregion

        #region IDisposable

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHostingPeer

        void IHostingPeer.RegisterTargetPeer(string callbackUri)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        #endregion
    }
}