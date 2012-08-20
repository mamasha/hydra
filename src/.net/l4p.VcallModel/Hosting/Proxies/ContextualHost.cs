using System;
using System.ServiceModel;
using l4p.VcallModel.Target;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Hosting.Proxies
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

            void IHostingPeer.SubscribeTargets(TargetsInfo info) { Channel.SubscribeTargets(info); }
            void IHostingPeer.CancelTargets(string targetsTag) { Channel.CancelTargets(targetsTag); }
        }
    }
}