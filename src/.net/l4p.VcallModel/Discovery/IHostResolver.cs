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
    delegate void HostPeerNotification(string callbackUri, bool alive);

    class HostResolverException : Exception
    {
        public HostResolverException() { }
        public HostResolverException(string message) : base(message) { }
        public HostResolverException(string message, Exception inner) : base(message, inner) { }
    }

    class HostResolverConfiguration
    {
        public string ResolvingKey { get; set; }
        public int HelloMessageGap { get; set; }
        public int ByeMessageGap { get; set; }
        public string DiscoveryScopePattern { get; set; }
        public int DiscoveryOpening { get; set; }
        public int DiscoveryClosing { get; set; }
    }

    interface IHostResolver
    {
        void Start();
        void Stop();

        // user arbitrary threads

        void PublishHostingPeer(string callbackUri, ICommNode node);
        void SubscribeTargetPeer(HostPeerNotification notify, ICommNode node);

        void CancelPublishedHosting(ICommNode node);
        void CancelSubscribedTarget(ICommNode node);

        DebugCounters DebugCounters { get; }

        // WCF arbitrary threads

        void HandleHelloMessage(EndpointDiscoveryMetadata edm, DateTime lastSeen);

        // resolver thread

        void SendHelloMessages();
        void GenerateByeNotifications(DateTime now);
    }
}