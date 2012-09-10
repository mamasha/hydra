/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using l4p.VcallModel.Core;
using l4p.VcallModel.HostingPeers;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.ProxyPeers.Channels
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
                _counters.ProxyPeer_Event_NewWcfChannel++;
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
                    _counters.ProxyPeer_Error_HostingCalls++;

                throw;
            }
        }

        #endregion

        void IHostingPeer.SubscribeProxy(ProxyInfo info) { call(() => _channel.SubscribeProxy(info)); }
        void IHostingPeer.CancelProxy(string proxyTag) { call(() => _channel.CancelProxy(proxyTag)); }
    }

    class WcfChannel : ClientBase<IHostingPeer>, IHostingPeer
    {
        public WcfChannel(Binding binding, EndpointAddress epoint) :
            base(binding, epoint)
        { }

        void IHostingPeer.SubscribeProxy(ProxyInfo info) { Channel.SubscribeProxy(info); }
        void IHostingPeer.CancelProxy(string proxyTag) { Channel.CancelProxy(proxyTag); }
    }
}