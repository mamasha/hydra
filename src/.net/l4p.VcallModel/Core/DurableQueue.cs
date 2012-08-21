/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Core
{
    interface IDurableQueue
    {
        void Add(IDurableOperation op);
        IDurableOperation[] ReadyDurables(DateTime now);
        IDurableOperation[] DeadDurables(DateTime now);
        IDurableOperation[] FindCanceled(string cancelationTag);
        int CalcNextTimeout(DateTime now);
        void Remove(IDurableOperation op);

        IDurableOperation[] GetAll();

        int Count { get; }
    }

    class DurableQueue : IDurableQueue
    {
        #region members

        private static readonly ILogger _log = Logger.New<DurableQueue>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly LinkedList<IDurableOperation> _ops;
        private readonly DebugCounters _counters;

        #endregion

        #region construction

        public static IDurableQueue New()
        {
            return
                new DurableQueue();
        }

        private DurableQueue()
        {
            _ops = new LinkedList<IDurableOperation>();
            _counters = Context.Get<ICountersDb>().NewCounters();
        }

        #endregion

        #region IDurableQueue

        void IDurableQueue.Add(IDurableOperation op)
        {
            _ops.AddLast(op);
            _counters.Vcall_State_DurableOperations++;
        }

        IDurableOperation[] IDurableQueue.ReadyDurables(DateTime now)
        {
            var bodies =
                from op in _ops
                where op.ToBeInvokedAt <= now
                select op;

            return
                bodies.ToArray();
        }

        IDurableOperation[] IDurableQueue.DeadDurables(DateTime now)
        {
            var bodies =
                from op in _ops
                where op.IsDone || op.IsFailed
                select op;

            return
                bodies.ToArray();
        }

        IDurableOperation[] IDurableQueue.FindCanceled(string cancelationTag)
        {
            var bodies =
                from op in _ops
                where op.CancelationTag == cancelationTag
                select op;

            return
                bodies.ToArray();
        }

        int IDurableQueue.CalcNextTimeout(DateTime now)
        {
            var nextTimeout = TimeSpan.MaxValue;
            bool found = false;

            foreach (var op in _ops)
            {
                if (op.IsDone)
                    continue;

                if (op.IsFailed)
                    continue;

                var timeout = op.ToBeInvokedAt - now;

                if (timeout < TimeSpan.Zero)
                    return 0;

                if (timeout < nextTimeout)
                    nextTimeout = timeout;

                found = true;
            }

            return
                found ? (int) (.5 + nextTimeout.TotalMilliseconds) : int.MaxValue;
        }

        void IDurableQueue.Remove(IDurableOperation op)
        {
            bool wasThere = _ops.Remove(op);

            if (wasThere)
                _counters.Vcall_State_DurableOperations--;
        }

        IDurableOperation[] IDurableQueue.GetAll()
        {
            return
                _ops.ToArray();
        }

        int IDurableQueue.Count
        {
            get { return _ops.Count; }
        }

        #endregion
    }
}