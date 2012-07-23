/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Threading;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Discovery
{
    interface IResolvingThread
    {
        void Start();
        void Stop();
    }

    class ResolvingThread : IResolvingThread
    {
        #region members

        private static readonly ILogger _log = Logger.New<ResolvingThread>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private readonly ManualResetEvent _stopSignal;
        private readonly ManualResetEvent _isStoppedEvent;
        private readonly Thread _thr;

        private readonly VcallConfiguration _config;
        private readonly IHostResolver _resolver;

        #endregion

        #region construction

        public ResolvingThread(IHostResolver resolver, VcallConfiguration config)
        {
            _config = config;
            _resolver = resolver;

            _stopSignal = new ManualResetEvent(false);
            _isStoppedEvent = new ManualResetEvent(false);
            _thr = new Thread(discovery_update_loop);
        }

        #endregion

        #region private

        private void discovery_update_loop()
        {
            _log.Info("Discovery update loop is started");

            try
            {
                int timeout = _config.Timeouts.DiscoveryUpdatePeriod;

                for (; ; )
                {
                    bool isStopped = _stopSignal.WaitOne(timeout);

                    if (isStopped)
                        break;

                    try
                    {
                        var now = DateTime.Now;

                        _resolver.SendHelloMessages();
                        _resolver.GenerateByeNotifications(now);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.GetDetailedStackTrace());
                    }
                }

                _isStoppedEvent.Set();
            }
            catch (Exception ex)
            {
                _log.Error(ex.GetDetailedMessage());
            }

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
            _stopSignal.Set();

            if (_isStoppedEvent.WaitOne(1000) == false)
            {
                _log.Info("Failed to stop announcement thread");
            }
        }

        #endregion
    }
}