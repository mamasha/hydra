/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.Core;
using l4p.VcallModel.HostingPeers;
using l4p.VcallModel.ProxyPeers;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.VcallSubsystems
{
    interface IEngine
    {
        HostingPeer NewHostingPeer(HostingConfiguration config, VcallSubsystem core);
        ProxyPeer NewProxyPeer(ProxyConfiguration config, VcallSubsystem core);
        void ClosePeer(int timeout, ICommPeer peer, IDoneEvent observer);
    }

    class Engine : IEngine
    {
        #region members

        private static readonly ILogger _log = Logger.New<Engine>();
        private static readonly IHelpers Helpers = HelpersInUse.All;
        private static readonly Internal _internalAccess = new Internal();

        private Self _self;

        #endregion

        #region construction

        public Engine(Self self)
        {
            _self = self;
        }

        #endregion

        #region private

        private void trace(string format, params object[] args)
        {
            if (_log.TraceIsOff)
                return;

            string msg = Helpers.SafeFormat(format, args);
            _log.Trace(msg);
        }

        private void warn(string format, params object[] args)
        {
            string msg = Helpers.SafeFormat(format, args);
            _log.Warn(msg);
        }

        private string make_dynamic_uri(string tag, string role)
        {
            string hostname = "localhost";
            int port = _self.vconfig.Port ?? Helpers.FindAvailableTcpPort();

            return
                String.Format(_self.vconfig.CallbackUriPattern, hostname, port, role, tag);
        }

        #endregion

        #region IEngine

        HostingPeer IEngine.NewHostingPeer(HostingConfiguration config, VcallSubsystem core)
            // user arbitrary thread
        {
            var timeout = TimeSpan.FromMilliseconds(_self.vconfig.Timeouts.HostingOpening);
            int addressInUseRetries = 0;

            for (;;)
            {
                var hosting = new HostingPeer(config, core);
                string uri = make_dynamic_uri(hosting.Tag, config.HostingRole);

                try
                {
                    hosting.Start(uri, timeout);
                    return hosting;
                }
                catch (Exception ex)
                {
                    if (ex.IsConsequenceOf<AddressAlreadyInUseException>())
                    {
                        addressInUseRetries++;

                        if (addressInUseRetries <= _self.vconfig.AddressInUseRetries)
                        {
                            warn("Dynamic URI '{0}' is in use; trying other one (retries={1})", uri, addressInUseRetries);
                            continue;
                        }

                        lock (_self.mutex)
                            _self.counters.Vcall_Error_AddressInUse++;

                        throw Helpers.MakeNew<VcallException>(ex, _log,
                            "hosting.{0}: Failed to listen on '{1}'; probably the TCP port is constantly in use (retries={2})", hosting.Tag, uri, addressInUseRetries);
                    }

                    lock (_self.mutex)
                        _self.counters.Vcall_Error_NewHostingFailed++;

                    throw;
                }
            }
        }

        ProxyPeer IEngine.NewProxyPeer(ProxyConfiguration config, VcallSubsystem core)
            // user arbitrary thread
        {
            var timeout = TimeSpan.FromMilliseconds(_self.vconfig.Timeouts.ProxyOpening);
            int addressInUseRetries = 0;

            for (;;)
            {
                var proxy = new ProxyPeer(config, core);
                string uri = make_dynamic_uri(proxy.Tag, config.ProxyRole);

                try
                {
                    proxy.Start(uri, timeout);

                    return proxy;
                }
                catch (Exception ex)
                {
                    if (ex.IsConsequenceOf<AddressAlreadyInUseException>())
                    {
                        addressInUseRetries++;

                        if (addressInUseRetries <= _self.vconfig.AddressInUseRetries)
                        {
                            warn("Dynamic URI '{0}' is in use; trying other one (retries={1})", uri, addressInUseRetries);
                            continue;
                        }

                        lock (_self.mutex)
                            _self.counters.Vcall_Error_AddressInUse++;

                        throw Helpers.MakeNew<VcallException>(ex, _log,
                            "proxy.{0}: Failed to listen on '{1}'; probably the TCP port is constantly in use (retries={2})", proxy.Tag, uri, addressInUseRetries);
                    }

                    lock (_self.mutex)
                        _self.counters.Vcall_Error_NewProxyFailed++;

                    throw;
                }
            }
        }

        void IEngine.ClosePeer(int timeout, ICommPeer peer, IDoneEvent observer)
            // user arbitrary thread
        {
            lock (_self.mutex)
            {
                _self.repo.Remove(peer);
                _self.counters.Vcall_Event_CloseCommPeer++;
            }

            _self.resolver.Cancel(peer.Tag);

            peer.Stop(timeout, observer);
        }

        #endregion

    }
}