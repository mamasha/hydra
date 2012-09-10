using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using l4p.VcallModel.Core;
using l4p.VcallModel.ProxyPeers;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.HostingPeers.Channels
{
    class ProxyPeer : IProxyPeer
    {
        #region members

        private readonly string _uri;
        private readonly Object _mutex;
        private readonly DebugCounters _counters;

        private IProxyPeer _channel;

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
                _counters.HostingPeer_Event_NewWcfChannel++;
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
                    _counters.HostingPeer_Error_ProxyCalls++;

                throw;
            }
        }

        #endregion

        #region construction

        public static IProxyPeer New(string uri)
        {
            return
                new ProxyPeer(uri);
        }

        private ProxyPeer(string uri)
        {
            _uri = uri;
            _mutex = new Object();
            _counters = Context.Get<ICountersDb>().NewCounters();

            _channel = null;
        }

        #endregion

        void IProxyPeer.SubscribeHosting(HostingInfo info) { call(() => _channel.SubscribeHosting(info)); }
        void IProxyPeer.CancelHosting(string hostingTag) { call(() => _channel.CancelHosting(hostingTag)); }
    }

    class WcfChannel : ClientBase<IProxyPeer>, IProxyPeer
    {
        public WcfChannel(Binding binding, EndpointAddress epoint) :
            base(binding, epoint)
        { }

        void IProxyPeer.SubscribeHosting(HostingInfo info) { Channel.SubscribeHosting(info); }
        void IProxyPeer.CancelHosting(string hostingTag) { Channel.CancelHosting(hostingTag); }
    }
}