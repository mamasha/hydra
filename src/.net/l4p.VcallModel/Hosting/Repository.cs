/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using l4p.VcallModel.Target;

namespace l4p.VcallModel.Hosting
{
    interface IRepository
    {
        void AddTarget(string callbackUri, ITargetPeer peer);
        void RemoveTarget(string callbackUri);
    }

    class Repository : IRepository
    {
        #region members

        private readonly IDictionary<string, ITargetPeer> _targets;

        #endregion

        #region construction

        public static IRepository New()
        {
            return
                new Repository();
        }

        private Repository()
        {
            _targets = new Dictionary<string, ITargetPeer>();
        }

        #endregion

        #region IRepository

        void IRepository.AddTarget(string callbackUri, ITargetPeer targetPeer)
        {
            throw new NotImplementedException();
        }

        void IRepository.RemoveTarget(string callbackUri)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}