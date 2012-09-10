/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;

namespace l4p.VcallModel.ProxyPeers
{
    class ProxyPeerException : VcallModelException
    {
        public ProxyPeerException() { }
        public ProxyPeerException(string message) : base(message) { }
        public ProxyPeerException(string message, Exception inner) : base(message, inner) { }
    }

    [ServiceContract]
    interface IProxyPeer
    {
        [OperationContract(IsOneWay = true)]
        void SubscribeHosting(HostingInfo info);

        [OperationContract(IsOneWay = true)]
        void CancelHosting(string hostingTag);
    }
}