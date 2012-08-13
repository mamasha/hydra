/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace l4p.VcallModel.Core
{
    interface IActionQueue
    {
        void Push(Action action);
        Action Pop(int timeout);
    }

    class ActionQueue : IActionQueue
    {
        #region members

        private Object _mutex;
        private Queue<Action> _que;

        #endregion

        #region construction

        public static IActionQueue New()
        {
            return
                new ActionQueue();
        }

        private ActionQueue()
        {
            _mutex = new Object();
            _que = new Queue<Action>();
        }

        #endregion

        #region private

        Action dequeue()
        {
            return
                _que.Count > 0 ? _que.Dequeue() : null;
        }

        int enqueue(Action action)
        {
            _que.Enqueue(action);
            return
                _que.Count;
        }

        #endregion

        #region IActionQueue

        void IActionQueue.Push(Action action)
        {
            lock (_mutex)
            {
                if (enqueue(action) == 1)
                    Monitor.Pulse(_mutex);
            }
        }

        Action IActionQueue.Pop(int timeout)
        {
            var tm = Stopwatch.StartNew();

            lock (_mutex)
            {
                for (;;)
                {
                    var action = dequeue();

                    if (action != null)
                        return action;

                    int timeLeft = timeout - (int) tm.ElapsedMilliseconds;

                    if (timeLeft <= 0)
                        return null;

                    Monitor.Wait(_mutex, timeLeft);
                }
            }
        }

        #endregion
    }
}