using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using l4p.VcallModel.Core;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Target.Proxies
{
    class HostingPeer : IHostingPeer
    {
        #region members

        private readonly string _uri;
        private readonly Object _mutex;
        private readonly DebugCounters _counters;

        private IHostingPeer _channel;

        #endregion

        #region construction

        public static IHostingPeer New(string uri)
        {
            return
                new HostingPeer(uri);
        }

        private HostingPeer(string uri)
        {
            _uri = uri;
            _mutex = new Object();
            _counters = Context.Get<ICountersDb>().NewCounters();
            _channel = null;
        }

        #endregion

        #region private

        private void ensure_channel_exists()
        {
            if (_channel != null)
                return;

            var binding = new TcpStreamBindng();
            var epoint = new EndpointAddress(_uri);

            _channel = new WcfChannel(binding, epoint);

            lock (_mutex)
                _counters.Targets_Event_NewWcfChannel++;
        }

        private void call(Action action)
        {
            ensure_channel_exists();

            try
            {
                action();
            }
            catch (Exception)
            {
                _channel = null;

                lock (_mutex)
                    _counters.Targets_Error_HostingCalls++;

                throw;
            }
        }

        #endregion

        void IHostingPeer.SubscribeTargets(TargetsInfo info) { call(() => _channel.SubscribeTargets(info)); }
        void IHostingPeer.CancelTargets(string targetsTag) { call(() => _channel.CancelTargets(targetsTag)); }
    }

    class WcfChannel : ClientBase<IHostingPeer>, IHostingPeer
    {
        public WcfChannel(Binding binding, EndpointAddress epoint) :
            base(binding, epoint)
        { }

        void IHostingPeer.SubscribeTargets(TargetsInfo info) { Channel.SubscribeTargets(info); }
        void IHostingPeer.CancelTargets(string targetsTag) { Channel.CancelTargets(targetsTag); }
    }
}