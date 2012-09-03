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
    class CommPeerException : VcallModelException
    {
        public CommPeerException() { }
        public CommPeerException(string message) : base(message) { }
        public CommPeerException(string message, Exception inner) : base(message, inner) { }
    }

    interface ICommPeer
    {
        string Tag { get; }
        void Stop(int timeout, IDoneEvent observer);
    }

    abstract class CommPeer : ICommPeer
    {
        #region helpers

        protected enum State { None, Started, Stopped }

        #endregion

        #region members

        private static readonly ILogger _log = Logger.New<CommPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        protected readonly string _tag;

        protected readonly DebugCounters _counters;

        protected State _state;

        #endregion

        #region construction

        protected CommPeer()
        {
            _tag = Helpers.GetRandomName();
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
                Helpers.MakeNew<CommPeerException>(null, _log, "Communication internal state is invalid; required={0} actual={1}", state, _state);
        }

        #endregion

        #region ICommPeer

        string ICommPeer.Tag
        {
            get { return _tag; }
        }

        void ICommPeer.Stop(int timeout, IDoneEvent observer)
        {
            Stop(TimeSpan.FromMilliseconds(timeout), observer);
        }

        #endregion
    }
}