/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.Target;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Hosting
{
    public class WcfHostringException : VcallModelException
    {
        public WcfHostringException() { }
        public WcfHostringException(string message) : base(message) { }
        public WcfHostringException(string message, Exception inner) : base(message, inner) { }
    }

    interface IWcfHostring
    {
        string Start(string uri, TimeSpan timeout);
        void Stop(TimeSpan timeout);
    }

    class WcfHosting : IWcfHostring
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
                ex => Helpers.ThrowNew<WcfHostringException>(ex, _log, "Failed to create hosting for '{0}'", typeof(IHostingPeer).Name));
        }

        #endregion

        #region IWcfHostring

        string IWcfHostring.Start(string uri, TimeSpan timeout)
        {
            Helpers.TryCatch(_log,
                () => _host.AddServiceEndpoint(typeof(IHostingPeer), new TcpStreamBindng(), uri),
                ex => Helpers.ThrowNew<WcfHostringException>(ex, _log, "Failed to add end point with uri '{0}'", uri));

            Helpers.TimedAction(
                () => _host.Open(timeout), "Failed to open hosting service in {0} millis", timeout.TotalMilliseconds);

            var listeningUri = _host.ChannelDispatchers[0].Listener.Uri;

            return listeningUri.ToString();
        }

        void IWcfHostring.Stop(TimeSpan timeout)
        {
            Helpers.TimedAction(
                () => _host.Close(timeout), "Failed to close hosting service in {0} millis", timeout.TotalMilliseconds);
        }

        #endregion
    }

    class ContextualHost : ServiceHost
    {
        private IHostingPeer _channel;

        public ContextualHost(IHostingPeer channel)
            : base(typeof(ContextualPeer))
        {
            _channel = channel;
        }

        class ContextualPeer : IHostingPeer
        {
            private static IHostingPeer Channel
            {
                get
                {
                    var context = (ContextualHost)OperationContext.Current.Host;
                    return context._channel;
                }
            }

            void IHostingPeer.SubscribeTargets(TargetsInfo info) { Channel.SubscribeTargets(info); }
            void IHostingPeer.CancelTargets(string targetsTag) { Channel.CancelTargets(targetsTag); }
        }
    }

}