/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

namespace l4p.VcallModel
{
    public enum NonRegisteredCallPolicy
    {
        Default,
        ThrowException,
        IgnoreCall
    }

    public class TargetConfiguration
    {
        /// <summary>
        /// A namespace of v-calls targets</summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// TCP port to connect to  </summary>
        public int? Port { get; set; }

        public string TargetsRole { get; set; }
        public string HostingRole { get; set; }
        public NonRegisteredCallPolicy NonRegisteredCall { get; set; }

        public int? SubscribeToHosting_RetryTimeout { get; set; }
    }
}