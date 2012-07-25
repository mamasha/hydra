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
using l4p.VcallModel.Helpers;

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

        private static readonly ILogger _log = Logger.New<WcfDiscovery>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private VcallConfiguration _config;
        private IHostResolver _resolver;

        private readonly ServiceEndpoint _serviceEndpoint;
        private readonly ServiceHost _announcementService;
        private readonly AnnouncementClient _announcementCleint;

        #endregion

        #region construction

        public WcfDiscovery(IHostResolver resolver, VcallConfiguration config)
        {
            _config = config;
            _resolver = resolver;

            var listener = new AnnouncementService();

            listener.OnlineAnnouncementReceived +=
                (sender, args) => handle_hello_message(args.EndpointDiscoveryMetadata);

            _serviceEndpoint = new UdpAnnouncementEndpoint();
            _announcementService = new ServiceHost(listener);
            _announcementService.AddServiceEndpoint(_serviceEndpoint);

            _announcementCleint = new AnnouncementClient(new UdpAnnouncementEndpoint());
        }

        #endregion

        #region private

        private void handle_hello_message(EndpointDiscoveryMetadata edm)
        {
            try
            {
                _resolver.HandleHelloMessage(edm);
            }
            catch (Exception ex)
            {
                _log.Error(ex.GetDetailedStackTrace());
            }
        }

        #endregion

        #region IWcfDiscovery

        EndpointAddress IWcfDiscovery.Address
        {
            get { return _serviceEndpoint.Address; }
        }

        void IWcfDiscovery.Start()
        {
            var timeout = Helpers.MakeTimeSpan(_config.Timeouts.DiscoveryOpening);

            Helpers.TimedAction(
                () => _announcementService.Open(timeout), "Failed to open announcement service in {0} millis", timeout.TotalMilliseconds);

            _announcementCleint.Open();

            _log.Info("WCF discovery module is started");
        }

        void IWcfDiscovery.Stop()
        {
            var timeout = Helpers.MakeTimeSpan(_config.Timeouts.DiscoveryClosing);

            Helpers.TimedAction(
                () => _announcementService.Close(timeout), "Failed to close announcement service in {0} millis", timeout.TotalMilliseconds);

            _log.Info("WCF discovery module is stopped");
        }

        void IWcfDiscovery.SendHelloMessage(EndpointDiscoveryMetadata edm)
        {
            _announcementCleint.AnnounceOnlineAsync(edm);
        }

        #endregion
    }
}