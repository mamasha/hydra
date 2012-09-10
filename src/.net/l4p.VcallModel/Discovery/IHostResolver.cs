/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;

namespace l4p.VcallModel.Discovery
{
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

    delegate void PubSubHandler(string callbackUri, string role, bool alive);

    interface IHostResolver
    {
        void Start();
        void Stop();

        void Publish(string callbackUri, string role, string tag);
        void Subscribe(PubSubHandler onPubSub, string tag);
        void Cancel(string tag);
    }
}