/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using l4p.VcallModel.Core;
using l4p.VcallModel.Target;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Hosting
{
    interface IRepository
    {
        void AddTargets(TargetsInfo info);
        void RemoveTargets(string tag);
        bool HasTargets(TargetsInfo info);
        int TargetsCount { get; }

        ITargetsPeer GetTargets(string tag);

        void CleanUp(string id);
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<HostingPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly ICommNode _hosting;

        private IDictionary<string, ITargetsPeer> _targets;

        #endregion

        #region construction

        public static IRepository New(CommNode hosting)
        {
            return
                new Repository(hosting);
        }

        private Repository(ICommNode hostring)
        {
            _hosting = hostring;
            _targets = new Dictionary<string, ITargetsPeer>();
        }

        #endregion

        #region IRepository

        void IRepository.AddTargets(TargetsInfo info)
        {
            throw
                Helpers.NewNotImplementedException();

            var targets = _targets;

            if (targets.ContainsKey(info.Tag))
            {
                _log.Warn("hosting.{0}: target '{1}' is already here", _hosting.Tag, info.Tag);
            }

            var newTargets = new Dictionary<string, ITargetsPeer>(targets);
//            targets[info.Tag] = targetsPeer;

            _targets = newTargets;
        }

        void IRepository.RemoveTargets(string tag)
        {
            var targets = _targets;

            var newTargets = new Dictionary<string, ITargetsPeer>(targets);
            bool wasHere = newTargets.Remove(tag);

            if (!wasHere)
            {
                _log.Warn("hosting.{0}: target '{1}' was not here", _hosting.Tag, tag);
            }

            _targets = newTargets;
        }

        bool IRepository.HasTargets(TargetsInfo info)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        int IRepository.TargetsCount
        {
            get
            {
                throw
                    Helpers.NewNotImplementedException();
            }
        }

        ITargetsPeer IRepository.GetTargets(string tag)
        {
            var targets = _targets;

            ITargetsPeer target;

            if (_targets.TryGetValue(tag, out target) == false)
            {
                _log.Warn("hosting.{0}: target '{1}' is not here", _hosting.Tag, tag);
                return null;
            }

            return target;
        }

        void IRepository.CleanUp(string id)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        #endregion
    }
}