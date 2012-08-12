/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Discovery
{
    interface IResolvingThread
    {
        void Start();
        void Stop();
        void PostAction(Action action);

        DebugCounters Counters { get; }
    }

    class ResolvingThread : IResolvingThread
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostResolver>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly BlockingCollection<Action> _todos;
        private readonly ManualResetEvent _isStoppedEvent;
        private readonly Thread _thr;

        private readonly int _helloMsgGap;
        private readonly IEngine _engine;

        private readonly Stopwatch _groovyTimer;
        private bool _stopFlagIsOn;

        private readonly DebugCounters _counters;

        #endregion

        #region construction

        public ResolvingThread(Self self, IEngine engine)
        {
            _helloMsgGap = self.config.HelloMessageGap;
            _engine = engine;

            _todos = new BlockingCollection<Action>();
            _isStoppedEvent = new ManualResetEvent(false);

            _thr = new Thread(discovery_update_main)
                       {
                           Name = "l4p.VcallModel.ResolvingThread"
                       };

            _stopFlagIsOn = false;
            _groovyTimer = new Stopwatch();

            _counters = new DebugCounters();
        }

        #endregion

        #region private

        private int do_groovy_tasks()
        {
            long nextMoment = _helloMsgGap - _groovyTimer.ElapsedMilliseconds;

            if (nextMoment > 0)
                return (int) nextMoment;

            try
            {
                _engine.SendHelloMessages();
                _engine.GenerateByeNotifications(DateTime.Now);
            }
            catch (Exception ex)
            {
                _log.Error(ex.GetDetailedStackTrace());
            }

            _groovyTimer.Restart();

            return _helloMsgGap;
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

        private void discovery_update_loop()
        {
            _groovyTimer.Start();

            for (;;)
            {
                int timeout = do_groovy_tasks();

                for (;;)
                {
                    Action action;

                    if (_todos.TryTake(out action, timeout) == false)
                        break;

                    do_action_request(action);

                    if (_stopFlagIsOn)
                        break;
                }

                if (_stopFlagIsOn)
                    break;
            }
        }

        private void discovery_update_main()
        {
            _log.Info("Discovery update loop is started");

            try
            {
                discovery_update_loop();
            }
            catch (Exception ex)
            {
                _log.Error(ex.GetDetailedMessage());
            }

            _isStoppedEvent.Set();

            _log.Info("Discovery update loop is done");
        }

        #endregion

        #region IResolvingThread

        void IResolvingThread.Start()
        {
            _thr.Start();
        }

        void IResolvingThread.Stop()
        {
            _todos.Add(() => _stopFlagIsOn = true);

            if (_isStoppedEvent.WaitOne(1000) == false)
            {
                _log.Info("Failed to stop announcement thread");
            }
        }

        void IResolvingThread.PostAction(Action action)
        {
            _todos.Add(action);
        }

        DebugCounters IResolvingThread.Counters
        {
            get { return _counters; }
        }

        #endregion
    }
}