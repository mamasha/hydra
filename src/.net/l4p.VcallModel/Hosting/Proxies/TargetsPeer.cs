using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using l4p.VcallModel.Core;
using l4p.VcallModel.Target;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Hosting.Proxies
{
    class TargetsPeer : ITargetsPeer
    {
        #region members

        private readonly string _uri;
        private readonly Object _mutex;
        private readonly DebugCounters _counters;

        private ITargetsPeer _channel;

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
                _counters.Hosting_Event_NewWcfChannel++;
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
                    _counters.Hosting_Error_TargetsCalls++;

                throw;
            }
        }

        #endregion

        #region construction

        public static ITargetsPeer New(string uri)
        {
            return
                new TargetsPeer(uri);
        }

        private TargetsPeer(string uri)
        {
            _uri = uri;
            _mutex = new Object();
            _counters = Context.Get<ICountersDb>().NewCounters();

            _channel = null;
        }

        #endregion

        void ITargetsPeer.SubscribeHosting(HostingInfo info) { call(() => _channel.SubscribeHosting(info)); }
        void ITargetsPeer.CancelHosting(string hostingTag) { call(() => _channel.CancelHosting(hostingTag)); }
    }

    class WcfChannel : ClientBase<ITargetsPeer>, ITargetsPeer
    {
        public WcfChannel(Binding binding, EndpointAddress epoint) :
            base(binding, epoint)
        { }

        void ITargetsPeer.SubscribeHosting(HostingInfo info) { Channel.SubscribeHosting(info); }
        void ITargetsPeer.CancelHosting(string hostingTag) { Channel.CancelHosting(hostingTag); }
    }
}