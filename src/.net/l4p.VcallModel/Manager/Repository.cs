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
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Manager
{
    interface IRepository
    {
        void Add(IVhosting node);
        void Add(IVtarget node);
        void Remove(ICommNode node);
        ICommNode[] GetNodes();
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<Repository>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly DebugCounters _counters;

        private List<ICommNode> _nodes;

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
            _nodes = new List<ICommNode>();
        }

        #endregion

        #region private

        private void add_node(ICommNode node)
        {
            var nodes = _nodes;

            var newNodes = new List<ICommNode>(nodes);
            newNodes.Add(node);

            _counters.Vcall_State_ActiveNodes++;

            _nodes = newNodes;
        }

        private void remove_node(ICommNode nodeToRemove)
        {
            var nodes = _nodes;

            var newNodes =
                from node in nodes
                where !ReferenceEquals(node, nodeToRemove)
                select node;

            _counters.Vcall_State_ActiveNodes--;

            _nodes = newNodes.ToList();
        }

        #endregion

        #region IRepository

        void IRepository.Add(IVhosting node)
        {
            add_node(node);
        }

        void IRepository.Add(IVtarget node)
        {
            add_node(node);
        }

        void IRepository.Remove(ICommNode node)
        {
            remove_node(node);
        }

        ICommNode[] IRepository.GetNodes()
        {
            var nodes = _nodes;

            return
                nodes.ToArray();
        }

        #endregion
    }
}