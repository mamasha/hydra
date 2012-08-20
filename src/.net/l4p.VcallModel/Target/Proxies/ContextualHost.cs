using System.ServiceModel;
using l4p.VcallModel.Hosting;

namespace l4p.VcallModel.Target.Proxies
{
    class ContextualHost : ServiceHost
    {
        private ITargetsPeer _channel;

        public ContextualHost(ITargetsPeer channel)
            : base(typeof(ContextualPeer))
        {
            _channel = channel;
        }

        class ContextualPeer : ITargetsPeer
        {
            private static ITargetsPeer Channel
            {
                get
                {
                    var context = (ContextualHost)OperationContext.Current.Host;
                    return context._channel;
                }
            }

            void ITargetsPeer.SubscribeHosting(HostingInfo info) { Channel.SubscribeHosting(info); }
            void ITargetsPeer.CancelHosting(string hostingTag) { Channel.CancelHosting(hostingTag); }
        }
    }
}