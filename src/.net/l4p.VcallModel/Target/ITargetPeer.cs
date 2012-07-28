/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Target
{
    class TargetPeerException : Exception
    {
        public TargetPeerException() { }
        public TargetPeerException(string message) : base(message) { }
        public TargetPeerException(string message, Exception inner) : base(message, inner) { }
    }

    [ServiceContract]
    interface ITargetPeer
    {
        [OperationContract(IsOneWay = true)]
        void HostIsOpened(string hostTag);

        [OperationContract(IsOneWay = true)]
        void HostIsClosed(string hostTag);
    }

    class TargetPeerProxy : ClientBase<ITargetPeer>, ITargetPeer
    {
        public static ITargetPeer New(string uri)
        {
            return
                new TargetPeerProxy(uri);
        }

        private TargetPeerProxy(String uri) :
            base(new TcpStreamBindng(), new EndpointAddress(uri))
        { }

        void ITargetPeer.HostIsOpened(string hostTag) { Channel.HostIsOpened(hostTag); }
        void ITargetPeer.HostIsClosed(string hostTag) { Channel.HostIsClosed(hostTag); }
    }
}