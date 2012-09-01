/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System.Collections.Generic;
using System.Linq;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.HostingPeers
{
    interface IRepository
    {
        int ProxyCount { get; }
        bool HasProxy(string tag);

        void AddProxy(ProxyInfo info);
        void RemoveProxy(ProxyInfo info);

        ProxyInfo FindProxy(string tag);
        ProxyInfo GetProxy(string tag);
        ProxyInfo[] GetProxies();
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostingPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly IDictionary<string, ProxyInfo> _proxies;
        private readonly DebugCounters _counters;

        #endregion

        #region construction

        public static IRepository New()
        {
            return
                new Repository();
        }

        private Repository()
        {
            _proxies = new Dictionary<string, ProxyInfo>();
            _counters = Context.Get<ICountersDb>().NewCounters();
        }

        #endregion

        #region private

        private ProxyInfo find_proxy(string tag)
        {
            var proxies =
                from proxy in _proxies.Values
                where proxy.Tag == tag || proxy.ListeningUri == tag
                select proxy;

            return
                proxies.FirstOrDefault();
        }

        #endregion

        #region IRepository

        int IRepository.ProxyCount
        {
            get { return _proxies.Count; }
        }

        bool IRepository.HasProxy(string tag)
        {
            return
                _proxies.ContainsKey(tag);
        }

        void IRepository.AddProxy(ProxyInfo info)
        {
            _proxies[info.Tag] = info;
            _counters.Hosting_State_AliveProxies++;
        }

        void IRepository.RemoveProxy(ProxyInfo info)
        {
            bool wasThere = _proxies.Remove(info.Tag);

            if (wasThere)
                _counters.Hosting_State_AliveProxies--;
        }

        ProxyInfo IRepository.FindProxy(string tag)
        {
            return
                find_proxy(tag);
        }

        ProxyInfo IRepository.GetProxy(string tag)
        {
            var proxy = find_proxy(tag);

            if (proxy == null)
            {
                throw
                    Helpers.MakeNew<HostingPeerException>(null, _log, "Can't find proxy.{0}", tag);
            }

            return proxy;
        }

        ProxyInfo[] IRepository.GetProxies()
        {
            return
                _proxies.Values.ToArray();
        }

        #endregion
    }
}