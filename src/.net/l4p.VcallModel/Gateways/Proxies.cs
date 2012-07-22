/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Gateways
{
    class HostingPeerProxy : ClientBase<IHostingPeer>, IHostingPeer
    {
        public HostingPeerProxy(String uri) :
            base(new TcpStreamBindng(), new EndpointAddress(uri))
        { }

        void IHostingPeer.RegisterTargetPeer(string callbackUri) { Channel.RegisterTargetPeer(callbackUri); }
    }

    class TargetPeerProxy : ClientBase<ITargetPeer>, ITargetPeer
    {
        public TargetPeerProxy(String uri) :
            base(new TcpStreamBindng(), new EndpointAddress(uri))
        { }

        void ITargetPeer.UpdateSubjects() { Channel.UpdateSubjects(); }
    }
}