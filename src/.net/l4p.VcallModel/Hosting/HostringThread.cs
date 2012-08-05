using System;
using System.Collections.Concurrent;
using System.Threading;
using l4p.VcallModel.Discovery;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Hosting
{
    interface IHostingThread
    {
        void Start();
        void Stop();
        void PostAction(Action action);
        void PostAction(Action action, string format, params object[] args);
    }

    class HostringThread : IHostingThread
    {
        #region members

        private static readonly ILogger _log = Logger.New<ResolvingThread>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly Thread _thr;
        private readonly BlockingCollection<Action> _todos;
        private readonly ManualResetEvent _isStoppedEvent;

        private readonly string _tag;

        private bool _stopFlagIsOn;

        #endregion

        #region construction

        public static IHostingThread New(string tag)
        {
            return
                new HostringThread(tag);
        }

        private HostringThread(string tag)
        {
            _tag = tag;

            _todos = new BlockingCollection<Action>();
            _isStoppedEvent = new ManualResetEvent(false);
            _thr = new Thread(hosting_main);

            _stopFlagIsOn = false;
        }

        #endregion

        #region private

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

        private void hosting_maintenance_loop()
        {
            for (;;)
            {
                int timeout = 1000;

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

        private void hosting_main()
        {
            _log.Info("hosting.{0}: Discovery update loop is started", _tag);

            try
            {
                hosting_maintenance_loop();
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _log.Error(ex.GetDetailedMessage(), "hosting.{0}: Unexpected exception in hosting thread", _tag);
            }

            _isStoppedEvent.Set();

            _log.Info("hosting.{0}: Discovery update loop is done", _tag);
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

        #endregion

        #region IHostingThread

        void IHostingThread.Start()
        {
            _thr.Start();
        }

        void IHostingThread.Stop()
        {
            _todos.Add(() => _stopFlagIsOn = true);

            if (_isStoppedEvent.WaitOne(1000) == false)
            {
                _log.Info("hosting.{0}: Failed to stop hostring thread", _tag);
            }
        }

        void IHostingThread.PostAction(Action action)
        {
            _todos.Add(action);
        }

        void IHostingThread.PostAction(Action action, string format, params object[] args)
        {
            _todos.Add(
                () => action_with_comment(action, format, args));
        }

        #endregion
    }
}