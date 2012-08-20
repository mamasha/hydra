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
        void Stop(Action stopTail);

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
            public string Name { get; set; }
            public int MaxAwaitTimeout { get; set; }
            public int FailureTimeout { get; set; }
            public int StartTimeout { get; set; }
            public int StopTimeout { get; set; }

            public Config()
            {
                Name = "ActiveThread";
                MaxAwaitTimeout = 1000;
                FailureTimeout = 60000;
                StartTimeout = 2000;
                StopTimeout = 1000;
            }
        }

        #endregion

        #region members

        private static readonly ILogger _log = Logger.New<ActiveThread>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly Thread _thr;
        private readonly IActionQueue _que;
        private readonly IDurableQueue _durables;
        private readonly ManualResetEvent _isStartedEvent;
        private readonly ManualResetEvent _isStoppedEvent;

        private readonly Config _config;

        private readonly DebugCounters _counters;

        private bool _stopFlagIsOn;

        #endregion

        #region construction

        public static IActiveThread New(Config config = null)
        {
            return
                new ActiveThread(config ?? new Config());
        }

        private ActiveThread(Config config)
        {
            _config = config;

            _que = ActionQueue.New();
            _durables = DurableQueue.New();
            _isStartedEvent = new ManualResetEvent(false);
            _isStoppedEvent = new ManualResetEvent(false);

            _thr = new Thread(main) { Name = _config.Name };

            _counters = Context.Get<ICountersDb>().NewCounters();

            _stopFlagIsOn = false;
        }

        #endregion

        #region private

        private void info(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Info("{0}: {1}", _config.Name, msg);
        }

        private void trace(string format, params object[] args)
        {
            if (_log.TraceIsOff)
                return;

            string msg = Helpers.SafeFormat(format, args);
            _log.Trace("{0}: {1}", _config.Name, msg);
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
                    _log.Error(durable.LastError, "'{0}' has permanently failed (retries={1}); {2}", durable.Comments, durable.FailureCount, durable.LastErrorMsg);
                }

                _durables.Remove(durable);
            }
        }

        private void maintenance_loop()
        {
            _isStartedEvent.Set();

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

                var action = _que.Pop(timeout);

                if (action != null)
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
                _log.Error(ex.GetDetailedStackTrace(), "{0}: Unexpected exception", _config.Name);
            }

            info("update loop is done");

            _isStoppedEvent.Set();
        }

        #endregion

        #region IActiveThread

        void IActiveThread.Start()
        {
            _thr.Start();

            if (_isStartedEvent.WaitOne(_config.StartTimeout) == false)
            {
                throw
                    Helpers.MakeNew<ActiveThreadException>(null, _log, "{0}: Failed to start the underlaying thread (timeout={1})", _config.Name, _config.StartTimeout);
            }
        }

        void IActiveThread.Stop()
        {
            _que.Push(() => _stopFlagIsOn = true);
        }

        void IActiveThread.Stop(Action stopTail)
        {
            _que.Push(() =>
                          {
                              _stopFlagIsOn = true;
                              stopTail();
                          });
        }

        void IActiveThread.PostAction(Action action)
        {
            _que.Push(action);
        }

        void IActiveThread.PostAction(Action action, string format, params object[] args)
        {
            _que.Push(
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
            get { return _counters; }
        }

        #endregion
    }
}