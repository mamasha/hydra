/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Discovery;
using l4p.VcallModel.Manager;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Connectivity
{
    interface IConnectivity
    {
        void Start();
        void Stop();

        void Subscribe(PubSubEvent onPubSub);

        void NotifyPubSubMsg(string callbackUri, string role, bool alive);
        void NotifyConnectionFailure();
    }

    class ConnectivityManager : IConnectivity
    {
        #region members

        private static readonly ILogger _log = Logger.New<ConnectivityManager>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            if (_log.TraceIsOff)
                return;

            string msg = Helpers.SafeFormat(format, args);
            _log.Trace(msg);
        }

        #endregion

        #region construction

        public static IConnectivity New()
        {
            return
                new ConnectivityManager();
        }

        private ConnectivityManager()
        { }

        #endregion

        #region IConnectivityManager

        void IConnectivity.Start()
        {
            trace("connectivity is started");
        }

        void IConnectivity.Stop()
        {
            trace("connectivity is stopped");
        }

        void IConnectivity.Subscribe(PubSubEvent onPubSub)
        {
            throw new NotImplementedException();
        }

        void IConnectivity.NotifyPubSubMsg(string callbackUri, string role, bool alive)
        {
        }

        void IConnectivity.NotifyConnectionFailure()
        {
        }

        #endregion
    }
}