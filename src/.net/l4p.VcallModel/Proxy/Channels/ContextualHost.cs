using System.ServiceModel;

namespace l4p.VcallModel.Proxy.Channels
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