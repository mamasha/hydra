/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Core;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel
{
    public interface ICommNode
    {
        /// <summary>
        /// A unique guid based tag (id) </summary>
        string Tag { get; }

        /// <summary>
        /// Get debug counters of this node </summary>
        DebugCounters DebugCounters { get; }

        /// <summary>
        /// Use Close() instead </summary>
        void Stop(Internal access, TimeSpan timeout);
    }
}