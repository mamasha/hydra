/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Discovery
{
    interface IWcfDiscovery
    {
        EndpointAddress Address { get; }

        void Start();
        void Stop();

        void SendHelloMessage(EndpointDiscoveryMetadata edm);
    }

    class WcfDiscovery : IWcfDiscovery
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostResolver>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly Self _self;
        private readonly IManager _engine;

        private readonly Object _mutex;
        private readonly ServiceEndpoint _serviceEndpoint;
        private readonly ServiceHost _announcementService;
        private readonly AnnouncementClient _announcementCleint;

        private readonly DebugCounters _counters;

        #endregion

        #region construction

        public WcfDiscovery(Self self, IManager engine)
        {
            _self = self;
            _engine = engine;

            var listener = new AnnouncementService();

            listener.OnlineAnnouncementReceived +=
                (sender, args) => handle_hello_message(args.EndpointDiscoveryMetadata);

            _mutex = new Object();
            _serviceEndpoint = new UdpAnnouncementEndpoint();
            _announcementService = new ServiceHost(listener);
            _announcementService.AddServiceEndpoint(_serviceEndpoint);

            _announcementCleint = new AnnouncementClient(new UdpAnnouncementEndpoint());

            _counters = Context.Get<ICountersDb>().NewCounters();
        }

        #endregion

        #region private

        private void handle_hello_message(EndpointDiscoveryMetadata edm)
        {
            string callbackUri;
            string role;

            lock (_mutex)
            {
                _counters.Discovery_Event_HelloMsgsRecieved++;

                if (edm.Scopes.Count != 1)
                {
                    _counters.Discovery_Event_HelloMsgsFiltered++;
                    return;
                }

                if (edm.ListenUris.Count != 1)
                {
                    _counters.Discovery_Event_HelloMsgsFiltered++;
                    return;
                }

                if (edm.ContractTypeNames.Count != 1)
                {
                    _counters.Discovery_Event_HelloMsgsFiltered++;
                    return;
                }

                Uri resolvingScope = edm.Scopes[0];

                if (resolvingScope != _self.resolvingScope)
                {
                    _counters.Discovery_Event_HelloMsgsFiltered++;
                    _counters.Discovery_Event_OtherHelloMsgsReceived++;
                    return;
                }

                callbackUri = edm.ListenUris[0].ToString();
                role = edm.ContractTypeNames[0].Name;

                _counters.Discovery_Event_MyHelloMsgsReceived++;
            }

            var lastSeen = DateTime.Now;

            _self.thread.PostAction(
                () => _engine.HandleHelloMessage(callbackUri, role, lastSeen));
        }

        #endregion

        #region IWcfDiscovery

        EndpointAddress IWcfDiscovery.Address
        {
            get { return _serviceEndpoint.Address; }
        }

        void IWcfDiscovery.Start()
        {
            var timeout = TimeSpan.FromMilliseconds(_self.config.DiscoveryOpening);

            Helpers.TimedAction(
                () => _announcementService.Open(timeout), "Failed to open announcement service in {0} millis", timeout.TotalMilliseconds);

            _announcementCleint.Open();

            _log.Info("WCF discovery module is started");
        }

        void IWcfDiscovery.Stop()
        {
            var timeout = TimeSpan.FromMilliseconds(_self.config.DiscoveryClosing);

            Helpers.TimedAction(
                () => _announcementService.Close(timeout), "Failed to close announcement service in {0} millis", timeout.TotalMilliseconds);

            _log.Info("WCF discovery module is stopped");
        }

        void IWcfDiscovery.SendHelloMessage(EndpointDiscoveryMetadata edm)
        {
            _announcementCleint.AnnounceOnlineAsync(edm);
            _counters.Discovery_Event_HelloMsgsSent++;
        }

        #endregion
    }
}