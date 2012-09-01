/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.ProxyPeers.Channels;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.ProxyPeers
{
    public class WcfProxyException : VcallModelException
    {
        public WcfProxyException() { }
        public WcfProxyException(string message) : base(message) { }
        public WcfProxyException(string message, Exception inner) : base(message, inner) { }
    }

    interface IWcfProxy
    {
        string Start(string uri, TimeSpan timeout);
        void Stop(TimeSpan timeout);
    }

    class WcfProxy : IWcfProxy
    {
        #region members

        private static readonly ILogger _log = Logger.New<WcfProxy>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private ServiceHost _host;

        #endregion
        
        #region construction

        public WcfProxy(IProxyPeer peer)
        {
            Helpers.TryCatch(_log,
                () => _host = new ContextualHost(peer),
                ex => Helpers.ThrowNew<WcfProxyException>(ex, _log, "Failed to create proxy for '{0}'", typeof(IProxyPeer).Name));
        }

        #endregion

        #region IWcfProxy

        string IWcfProxy.Start(string uri, TimeSpan timeout)
        {
            Helpers.TryCatch(_log,
                () => _host.AddServiceEndpoint(typeof(IProxyPeer), new TcpStreamBindng(), uri),
                ex => Helpers.ThrowNew<WcfProxyException>(ex, _log, "Failed to add end point with uri '{0}'", uri));

            Helpers.TimedAction(
                () => _host.Open(timeout), "Failed to open hosting service in {0} millis", timeout.TotalMilliseconds);

            var listeningUri = _host.ChannelDispatchers[0].Listener.Uri;

            return listeningUri.ToString();
        }

        void IWcfProxy.Stop(TimeSpan timeout)
        {
            Helpers.TimedAction(
                () => _host.Close(timeout), "Failed to close hosting service in {0} millis", timeout.TotalMilliseconds);
        }

        #endregion
    }
}