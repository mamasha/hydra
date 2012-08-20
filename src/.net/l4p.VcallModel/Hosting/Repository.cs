/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using l4p.VcallModel.Core;
using l4p.VcallModel.Target;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Hosting
{
    interface IRepository
    {
        int TargetsCount { get; }
        bool HasTargets(string tag);

        void AddTargets(TargetsInfo info);
        void RemoveTargets(TargetsInfo info);

        TargetsInfo FindTargets(string tag);
        TargetsInfo GetTargets(string tag);
        TargetsInfo[] GetTargets();
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostingPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly IDictionary<string, TargetsInfo> _targets;
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
            _targets = new Dictionary<string, TargetsInfo>();
            _counters = Context.Get<ICountersDb>().NewCounters();
        }

        #endregion

        #region private

        private TargetsInfo find_targets(string tag)
        {
            var targets =
                from target in _targets.Values
                where target.Tag == tag || target.ListeningUri == tag
                select target;

            return
                targets.FirstOrDefault();
        }

        #endregion

        #region IRepository

        int IRepository.TargetsCount
        {
            get { return _targets.Count; }
        }

        bool IRepository.HasTargets(string tag)
        {
            return
                _targets.ContainsKey(tag);
        }

        void IRepository.AddTargets(TargetsInfo info)
        {
            _targets[info.Tag] = info;
            _counters.Hosting_State_AliveTargets++;
        }

        void IRepository.RemoveTargets(TargetsInfo info)
        {
            bool wasThere = _targets.Remove(info.Tag);

            if (wasThere)
                _counters.Hosting_State_AliveTargets--;
        }

        TargetsInfo IRepository.FindTargets(string tag)
        {
            return
                find_targets(tag);
        }

        TargetsInfo IRepository.GetTargets(string tag)
        {
            var target = find_targets(tag);

            if (target == null)
            {
                throw
                    Helpers.MakeNew<HostingPeerException>(null, _log, "Can't find targets.{0}", tag);
            }

            return target;
        }

        TargetsInfo[] IRepository.GetTargets()
        {
            return
                _targets.Values.ToArray();
        }

        #endregion
    }
}