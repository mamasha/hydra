/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Concurrent;
using System.Threading;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Core
{
    public class ActiveThreadException : VcallModelException
    {
        public ActiveThreadException() { }
        public ActiveThreadException(string message) : base(message) { }
        public ActiveThreadException(string message, Exception inner) : base(message, inner) { }
    }

    interface IActiveThread
    {
        void Start();
        void Stop();

        void PostAction(Action action);
        void PostAction(Action action, string format, params object[] args);

        void DoOnce(int retryTimeout, string cancelationTag, Action action, string format, params object[] args);
        void Cancel(string cancelationTag);

        DebugCounters Counters { get; }
    }

    class ActiveThread : IActiveThread
    {
        #region config

        public class Config
        {
            public readonly int MaxAwaitTimeout = 1000;
            public readonly int FailureTimeout = 10000;
            public readonly int StopTimeout = 10000;
        }

        #endregion

        #region members

        private static readonly ILogger _log = Logger.New<ActiveThread>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly Thread _thr;
        private readonly BlockingCollection<Action> _todos;
        private readonly IDurableQueue _durables;
        private readonly ManualResetEvent _isStoppedEvent;

        private readonly string _name;
        private readonly Config _config;

        private bool _stopFlagIsOn;

        #endregion

        #region construction

        public static IActiveThread New(string name, Config config = null)
        {
            return
                new ActiveThread(name, config ?? new Config());
        }

        private ActiveThread(string name, Config config)
        {
            _name = name;
            _config = config;

            _todos = new BlockingCollection<Action>();
            _durables = DurableQueue.New();
            _isStoppedEvent = new ManualResetEvent(false);

            _thr = new Thread(main)
                       {
                           Name = name
                       };

            _stopFlagIsOn = false;
        }

        #endregion

        #region private

        private void info(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Info("{0}: {1}", _name, msg);
        }

        private void trace(string format, params object[] args)
        {
            if (_log.TraceIsOff)
                return;

            string msg = Helpers.SafeFormat(format, args);
            _log.Info("{0}: {1}", _name, msg);
        }

        private void assert_current_thread_is_mine()
        {
            if (ReferenceEquals(Thread.CurrentThread, _thr))
                return;

            throw
                Helpers.MakeNew<ActiveThreadException>(null, _log, "Method should be executed in a self thread");
        }

        private static void do_action_request(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _log.Error(ex.GetDetailedStackTrace());
            }
        }

        private void action_with_comment(Action action, string format, params object[] args)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                string errMsg = Helpers.SafeFormat(format, args);
                _log.Warn(ex, errMsg);
            }
        }

        private void clear_dead_durables(DateTime now)
        {
            var durables = _durables.DeadDurables(now);

            foreach (var durable in durables)
            {
                if (durable.IsFailed)
                {
                    _log.Error("'{0}' has permanently failed; (retries={1}); {2}", durable.Comments, durable.FailureCount, durable.LastErrMsg);
                }

                _durables.Remove(durable);
            }
        }

        private void maintenance_loop()
        {
            for (;;)
            {
                DateTime now = DateTime.Now;

                clear_dead_durables(now);
                var durables = _durables.ReadyDurables(now);

                foreach (var durable in durables)
                {
                    durable.Invoke();

                    if (_stopFlagIsOn)
                        break;
                }

                if (_stopFlagIsOn)
                    break;

                int timeout = 
                    Math.Min(_durables.CalcNextTimeout(DateTime.Now), _config.MaxAwaitTimeout);

                Action action;

                if (_todos.TryTake(out action, timeout))
                {
                    do_action_request(action);
                }

                if (_stopFlagIsOn)
                    break;
            }
        }

        private void main()
        {
            info("update loop is started");

            try
            {
                maintenance_loop();
            }
            catch (Exception ex)
            {
                _log.Error(ex.GetDetailedStackTrace(), "{0}: Unexpected exception", _name);
            }

            _isStoppedEvent.Set();

            info("update loop is done");
        }

        #endregion

        #region IActiveThread

        void IActiveThread.Start()
        {
            _thr.Start();
        }

        void IActiveThread.Stop()
        {
            _todos.Add(() => _stopFlagIsOn = true);

            if (_isStoppedEvent.WaitOne(_config.StopTimeout) == false)
            {
                _log.Info("{0}: Failed to stop targets thread", _name);
            }
        }

        void IActiveThread.PostAction(Action action)
        {
            _todos.Add(action);
        }

        void IActiveThread.PostAction(Action action, string format, params object[] args)
        {
            _todos.Add(
                () => action_with_comment(action, format, args));
        }

        void IActiveThread.DoOnce(int retryTimeout, string cancelationTag, Action action, string format, params object[] args)
        {
            assert_current_thread_is_mine();

            var durable = 
                DurableOperation.NewDoOnce(retryTimeout, _config.FailureTimeout, cancelationTag, action, format, args);

            if (durable.Invoke())
                return;

            _durables.Add(durable);
        }

        void IActiveThread.Cancel(string cancelationTag)
        {
            assert_current_thread_is_mine();

            var durables = _durables.FindCanceled(cancelationTag);

            foreach (var durable in durables)
            {
                _durables.Remove(durable);
            }

            trace("Canceled {0} durables; cancelationTag='{1}'", durables.Length, cancelationTag);
        }

        DebugCounters IActiveThread.Counters
        {
            get { return DebugCounters.AccumulateAll(_durables.Counters); }
        }

        #endregion
    }
}