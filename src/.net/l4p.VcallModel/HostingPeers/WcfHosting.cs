/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.HostingPeers.Channels;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.HostingPeers
{
    public class WcfHostingException : VcallModelException
    {
        public WcfHostingException() { }
        public WcfHostingException(string message) : base(message) { }
        public WcfHostingException(string message, Exception inner) : base(message, inner) { }
    }

    interface IWcfHosting
    {
        string Start(string uri, TimeSpan timeout);
        void Stop(TimeSpan timeout);
    }

    class WcfHosting : IWcfHosting
    {
        #region members

        private static readonly ILogger _log = Logger.New<WcfHosting>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private ServiceHost _host;

        #endregion

        #region construction

        public WcfHosting(IHostingPeer peer)
        {
            Helpers.TryCatch(_log,
                () => _host = new ContextualHost(peer),
                ex => Helpers.ThrowNew<WcfHostingException>(ex, _log, "Failed to create hosting for '{0}'", typeof(IHostingPeer).Name));
        }

        #endregion

        #region IWcfHosting

        string IWcfHosting.Start(string uri, TimeSpan timeout)
        {
            Helpers.TryCatch(_log,
                () => _host.AddServiceEndpoint(typeof(IHostingPeer), new TcpStreamBindng(), uri),
                ex => Helpers.ThrowNew<WcfHostingException>(ex, _log, "Failed to add end point with uri '{0}'", uri));

            Helpers.TimedAction(
                () => _host.Open(timeout), "Failed to open hosting service in {0} millis", timeout.TotalMilliseconds);

            var listeningUri = _host.ChannelDispatchers[0].Listener.Uri;

            return listeningUri.ToString();
        }

        void IWcfHosting.Stop(TimeSpan timeout)
        {
            Helpers.TimedAction(
                () => _host.Close(timeout), "Failed to close hosting service in {0} millis", timeout.TotalMilliseconds);
        }

        #endregion
    }
}