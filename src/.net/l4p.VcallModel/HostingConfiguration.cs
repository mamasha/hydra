/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

namespace l4p.VcallModel
{
    public class HostingConfiguration
    {
        /// <summary>
        /// TCP port to listen on for incoming calls </summary>
        public int? Port { get; set; }

        /// <summary>
        /// The resolving key of the host </summary>
        public string ResolvingKey { get; set; }
    }
}