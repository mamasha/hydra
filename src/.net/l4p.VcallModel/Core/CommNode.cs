/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.IO;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Core
{
    class CommNodeException : VcallModelException
    {
        public CommNodeException() { }
        public CommNodeException(string message) : base(message) { }
        public CommNodeException(string message, Exception inner) : base(message, inner) { }
    }

    abstract class CommNode : ICommNode
    {
        #region helpers

        protected enum State { None, Started, Stopped }

        #endregion

        #region members

        private static readonly ILogger _log = Logger.New<CommNode>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        protected readonly string _tag;

        protected readonly DebugCounters _counters;

        protected State _state;

        #endregion

        #region construction

        protected CommNode()
        {
            _tag = Helpers.RandomName8();
            _counters = Context.Get<ICountersDb>().NewCounters();
            _state = State.None;
        }

        #endregion

        #region protected api

        protected abstract void Close();
        protected abstract void Stop(TimeSpan timeout, IDoneEvent observer);

        protected void validate_state(State state)
        {
            if (_state == state)
                return;

            throw 
                Helpers.MakeNew<CommNodeException>(null, _log, "Communication internal state is invalid; required={0} actual={1}", state, _state);
        }

        #endregion

        #region ICommNode

        string ICommNode.Tag
        {
            get { return _tag; }
        }

        void ICommNode.Close()
        {
            Close();
        }

        void ICommNode.Stop(Internal access, int timeout, IDoneEvent observer)
        {
            InternalAccess.Check(access);
            Stop(TimeSpan.FromMilliseconds(timeout), observer);
        }

        #endregion
    }
}