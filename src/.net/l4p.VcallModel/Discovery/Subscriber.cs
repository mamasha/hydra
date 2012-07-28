/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace l4p.VcallModel.Discovery
{
    class Subscriber
    {
        public ICommNode Node { get; set; }
        public string ResolvingKey { get; set; }
        public HostPeerNotification Notify { get; set; }
        public Uri ResolvingScope { get; set; }
    }
}