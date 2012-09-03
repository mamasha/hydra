/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System.Collections.Generic;
using System.Linq;
using l4p.VcallModel.Core;
using l4p.VcallModel.HostingPeers;
using l4p.VcallModel.ProxyPeers;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.VcallSubsystems
{
    interface IRepository
    {
        void Add(ICommPeer peer);
        void Remove(ICommPeer peer);
        ICommPeer[] GetPeers();
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<Repository>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly DebugCounters _counters;

        private List<ICommPeer> _peers;

        #endregion

        #region construction

        public static IRepository New()
        {
            return 
                new Repository();
        }

        private Repository()
        {
            _counters = Context.Get<ICountersDb>().NewCounters();
            _peers = new List<ICommPeer>();
        }

        #endregion

        #region private

        private void add_peer(ICommPeer peer)
        {
            var peers = _peers;

            var newPeers = new List<ICommPeer>(peers);
            newPeers.Add(peer);

            _counters.Vcall_State_ActivePeers++;

            _peers = newPeers;
        }

        private void remove_peer(ICommPeer peerToRemove)
        {
            var peers = _peers;

            var newPeers =
                from peer in peers
                where !ReferenceEquals(peer, peerToRemove)
                select peer;

            _counters.Vcall_State_ActivePeers--;

            _peers = newPeers.ToList();
        }

        #endregion

        #region IRepository

        void IRepository.Add(ICommPeer peer)
        {
            add_peer(peer);
        }

        void IRepository.Remove(ICommPeer peer)
        {
            remove_peer(peer);
        }

        ICommPeer[] IRepository.GetPeers()
        {
            var peers = _peers;

            return
                peers.ToArray();
        }

        #endregion
    }
}