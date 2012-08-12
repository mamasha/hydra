/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Core
{
    abstract class CommNode : ICommNode
    {
        #region constants

        protected readonly int _subscribe_RetryTimeout = 1000;

        #endregion

        #region members

        public const string HostingRole = "hosting";
        public const string TargetsRole = "targets";

        protected readonly string _tag;

        protected DebugCounters _counters;

        #endregion

        #region construction

        protected CommNode()
        {
            _tag = Guid.NewGuid().ToString("B");
            _counters = new DebugCounters();
        }

        #endregion

        #region protected api

        protected abstract void Stop(TimeSpan timeout);

        #endregion

        #region ICommNode

        string ICommNode.Tag
        {
            get { return _tag; }
        }

        DebugCounters ICommNode.DebugCounters
        {
            get { return DebugCounters.AccumulateAll(_counters); }
        }

        void ICommNode.Stop(Internal access, TimeSpan timeout)
        {
            InternalAccess.Check(access);
            Stop(timeout);
        }

        #endregion
    }
}