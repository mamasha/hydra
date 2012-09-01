using System.ServiceModel;

namespace l4p.VcallModel.Hosting.Channels
{
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
                    var context = (ContextualHost) OperationContext.Current.Host;
                    return context._channel;
                }
            }

            void IHostingPeer.SubscribeProxy(ProxyInfo info) { Channel.SubscribeProxy(info); }
            void IHostingPeer.CancelProxy(string proxyTag) { Channel.CancelProxy(proxyTag); }
        }
    }
}