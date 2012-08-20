/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace l4p.VcallModel.Core
{
    interface ICountersDb
    {
        DebugCounters NewCounters();
        DebugCounters SumAll();
    }

    class CountersDb : ICountersDb
    {
        #region members

        private readonly Object _mutex;
        private readonly IList<DebugCounters> _all;

        #endregion

        #region construction

        public static ICountersDb New()
        {
            return
                new CountersDb();
        }

        private CountersDb()
        {
            _mutex = new Object();
            _all = new List<DebugCounters>();
        }

        #endregion

        #region ICountersDb

        DebugCounters ICountersDb.NewCounters()
        {
            var counters = new DebugCounters(this);

            lock (_mutex)
            {
                _all.Add(counters);
            }

            return counters;
        }

        DebugCounters ICountersDb.SumAll()
        {
            DebugCounters[] all;

            lock (_mutex)
            {
                all = _all.ToArray();
            }

            var sum = new DebugCounters(this);

            foreach (var counters in all)
            {
                sum.Accumulate(counters);
            }

            return sum;
        }

        #endregion
    }
}