/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.HostingPeers
{
    class Manager : IHosting
    {
        #region members

        private static readonly ILogger _log = Logger.New<Manager>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        #endregion

        #region construction
        #endregion

        #region ICommNode

        string ICommNode.Tag
        {
            get { throw new NotImplementedException(); }
        }

        void ICommNode.Close()
        {
            throw new NotImplementedException();
        }

        void ICommNode.Stop(Internal access, int timeout, IDoneEvent observer)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHosting

        void IHosting.Host(Action action)
        {
            throw new NotImplementedException();
        }

        void IHosting.Host<R>(Func<R> func)
        {
            throw new NotImplementedException();
        }

        void IHosting.Host<T1, T2>(string actionName, Action<T1, T2> action)
        {
            throw new NotImplementedException();
        }

        R IHosting.Host<T1, T2, R>(string funcName, Func<T1, T2, R> func)
        {
            throw new NotImplementedException();
        }

        void IHosting.Host<T1, T2>(Action<T1, T2> action)
        {
            throw new NotImplementedException();
        }

        void IHosting.Host<T1, R>(Func<T1, R> func)
        {
            throw new NotImplementedException();
        }

        void IHosting.Host<T1, T2, R>(Func<T1, T2, R> func)
        {
            throw new NotImplementedException();
        }

        string IHosting.ListeningUri
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}