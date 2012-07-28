/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Target
{
    public class WcfTargetException : Exception
    {
        public WcfTargetException() { }
        public WcfTargetException(string message) : base(message) { }
        public WcfTargetException(string message, Exception inner) : base(message, inner) { }
    }

    interface IWcfTarget
    {
        string Start(string uri, TimeSpan timeout);
        void Stop(TimeSpan timeout);
    }

    class WcfTarget : IWcfTarget
    {
        #region members

        private static readonly ILogger _log = Logger.New<WcfTarget>();
        private static readonly IHelpers Helpers = LoggedHelpers.New(_log);

        private ServiceHost _host;

        #endregion
        
        #region construction

        public WcfTarget(ITargetPeer peer)
        {
            Helpers.TryCatch(
                () => _host = new ContextualHost(peer),
                ex => Helpers.ThrowNew<WcfTargetException>(ex, "Failed to create target for '{0}'", typeof(ITargetPeer).Name));
        }

        #endregion

        #region IWcfTarget

        string IWcfTarget.Start(string uri, TimeSpan timeout)
        {
            Helpers.TryCatch(
                () => _host.AddServiceEndpoint(typeof(ITargetPeer), new TcpStreamBindng(), uri),
                ex => Helpers.ThrowNew<WcfTargetException>(ex, "Failed to add end point with uri '{0}'", uri));

            Helpers.TimedAction(
                () => _host.Open(timeout), "Failed to open hosting service in {0} millis", timeout.TotalMilliseconds);

            var listeningUri = _host.ChannelDispatchers[0].Listener.Uri;

            return listeningUri.ToString();
        }

        void IWcfTarget.Stop(TimeSpan timeout)
        {
            Helpers.TimedAction(
                () => _host.Close(timeout), "Failed to close hosting service in {0} millis", timeout.TotalMilliseconds);
        }

        #endregion
    }

    class ContextualHost : ServiceHost
    {
        private ITargetPeer _peer;

        public ContextualHost(ITargetPeer peer)
            : base(typeof(ContextualPeer))
        {
            _peer = peer;
        }

        class ContextualPeer : ITargetPeer
        {
            private ITargetPeer peer()
            {
                var context = (ContextualHost)OperationContext.Current.Host;
                return context._peer;
            }

            void ITargetPeer.HostIsOpened(string hostTag) { peer().HostIsOpened(hostTag); }
            void ITargetPeer.HostIsClosed(string hostTag) { peer().HostIsClosed(hostTag); }
        }

    }
}