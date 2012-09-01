/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System.ServiceModel;

namespace l4p.VcallModel.ProxyPeers.Channels
{
    class ContextualHost : ServiceHost
    {
        private IProxyPeer _channel;

        public ContextualHost(IProxyPeer channel)
            : base(typeof(ContextualPeer))
        {
            _channel = channel;
        }

        class ContextualPeer : IProxyPeer
        {
            private static IProxyPeer Channel
            {
                get
                {
                    var context = (ContextualHost)OperationContext.Current.Host;
                    return context._channel;
                }
            }

            void IProxyPeer.SubscribeHosting(HostingInfo info) { Channel.SubscribeHosting(info); }
            void IProxyPeer.CancelHosting(string hostingTag) { Channel.CancelHosting(hostingTag); }
        }
    }
}