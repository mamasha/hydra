/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Configuration;
using l4p.VcallModel.Connectivity;
using l4p.VcallModel.Core;
using l4p.VcallModel.Discovery;

namespace l4p.VcallModel.Manager
{
    class Self
    {
        public Object mutex { get; set; }
        public VcallConfiguration vconfig { get; set; }
        public IRepository repo { get; set; }
        public IHostResolver resolver { get; set; }
        public IConnectivity connectivity { get; set; }

        public DebugCounters counters { get; set; }
    }
}