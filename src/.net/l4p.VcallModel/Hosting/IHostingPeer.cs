/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Hosting
{
    class HostingPeerException : Exception
    {
        public HostingPeerException() { }
        public HostingPeerException(string message) : base(message) { }
        public HostingPeerException(string message, Exception inner) : base(message, inner) { }
    }

    [ServiceContract]
    interface IHostingPeer
    {
        [OperationContract]
        void RegisterTargetPeer(string targetTag, string callbackUri);

        [OperationContract(IsOneWay = true)]
        void UnregisterTargetPeer(string targetTag);
    }

    class HostingPeerProxy : ClientBase<IHostingPeer>, IHostingPeer
    {
        public static IHostingPeer New(string uri)
        {
            return new HostingPeerProxy(uri);
        }

        private HostingPeerProxy(string uri) :
            base(new TcpStreamBindng(), new EndpointAddress(uri))
        { }

        void IHostingPeer.RegisterTargetPeer(string targetTag, string callbackUri) { Channel.RegisterTargetPeer(targetTag, callbackUri); }
        void IHostingPeer.UnregisterTargetPeer(string targetTag) { Channel.UnregisterTargetPeer(targetTag); }
    }

}