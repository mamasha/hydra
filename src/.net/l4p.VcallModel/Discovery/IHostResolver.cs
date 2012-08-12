/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel.Discovery;
using l4p.VcallModel.Core;

namespace l4p.VcallModel.Discovery
{
    delegate void PublishNotification(string callbackUri, string role, bool alive);

    class HostResolverConfiguration
    {
        public string ResolvingKey { get; set; }
        public int HelloMessageGap { get; set; }
        public int ByeMessageGap { get; set; }
        public string DiscoveryScopePattern { get; set; }
        public int DiscoveryOpening { get; set; }
        public int DiscoveryClosing { get; set; }
    }

    class HostResolverException : VcallModelException
    {
        public HostResolverException() { }
        public HostResolverException(string message) : base(message) { }
        public HostResolverException(string message, Exception inner) : base(message, inner) { }
    }

    interface IHostResolver
    {
        void Start();
        void Stop();

        void Publish(string callbackUri, string role, string tag);
        void Subscribe(PublishNotification onPublish, string tag);
        void Cancel(string tag);

        DebugCounters Counters { get; }
    }
}