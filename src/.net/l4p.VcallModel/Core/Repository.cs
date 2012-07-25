﻿/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Core
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
        private static readonly IHelpers Helpers = Utils.New(_log);

        private readonly Object _mutex;
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
            _mutex = new Object();
            _nodes = new List<ICommNode>();
        }

        #endregion

        #region private

        private void add_node(ICommNode node)
        {
            var nodes = new List<ICommNode>(_nodes);
            nodes.Add(node);
            _nodes = nodes;
        }

        private void remove_node(ICommNode nodeToRemove)
        {
            var nodes =
                from node in _nodes
                where !ReferenceEquals(node, nodeToRemove)
                select node;

            _nodes = nodes.ToList();
        }

        #endregion

        #region IRepository

        void IRepository.Add(IVhosting node)
        {
            lock (_mutex)
            {
                add_node(node);
            }
        }

        void IRepository.Add(IVtarget node)
        {
            lock (_mutex)
            {
                add_node(node);
            }
        }

        void IRepository.Remove(ICommNode node)
        {
            lock (_mutex)
            {
                remove_node(node);
            }
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