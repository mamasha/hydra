﻿/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

namespace l4p.VcallModel
{
    /// <summary>
    /// Hosting visibility scope </summary>
    /// <remarks>Currently supported scopes are LocalHost and ResolvingRing</remarks>
    public enum HostingVisibilityScope
    {
        /// <summary>
        /// Default visibility scope is LocalHost </summary>
        Default,

        /// <summary>
        /// Hosting is visible to proxies withing current process only</summary>
        /// <remarks>Not supported yet</remarks>
        ProcessPrivate,

        /// <summary>
        /// Hosting is visible to proxies from process if current application domain only</summary>
        /// <remarks>Not supported yet</remarks>
        ApplicationDomain,

        /// <summary>
        /// Hosting is visible to proxies from any process running on local host machine</summary>
        LocalHost,

        /// <summary>
        /// Hosting is visible to proxies from any process with a same ResolvingKey running on a local Ethernet</summary>
        ResolvingDomain
    }

    public class HostingConfiguration
    {
        /// <summary>
        /// A namespace of hosted functions</summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// Defines which proxies will see functions hosted by this hosting </summary>
        /// <remarks>Currently supported scopes are LocalHost and ResolvingRing</remarks>
        public HostingVisibilityScope? VisibilityScope;

        /// <summary>
        /// Explicit TCP port to listen on for incoming calls 
        /// If not specified a random available port is chosen</summary>
        public int? Port { get; set; }

        public string ProxyRole { get; set; }
        public string HostingRole { get; set; }

        public int? SubscribeToProxy_RetryTimeout { get; set; }
    }
}