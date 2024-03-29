﻿/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.ServiceModel;

namespace l4p.VcallModel.HostingPeers
{
    class HostingPeerException : VcallModelException
    {
        public HostingPeerException() { }
        public HostingPeerException(string message) : base(message) { }
        public HostingPeerException(string message, Exception inner) : base(message, inner) { }
    }

    [ServiceContract]
    interface IHostingPeer
    {
        [OperationContract(IsOneWay = true)]
        void SubscribeProxy(ProxyInfo info);

        [OperationContract(IsOneWay = true)]
        void CancelProxy(string proxyTag);
    }
}