/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using l4p.VcallModel.Hosting;

namespace l4p.VcallModel.Target
{
    class TargetsPeerException : VcallModelException
    {
        public TargetsPeerException() { }
        public TargetsPeerException(string message) : base(message) { }
        public TargetsPeerException(string message, Exception inner) : base(message, inner) { }
    }

    [DataContract]
    class HostingInfo
    {
        [DataMember] public string Tag { get; set; }
        [DataMember] public string CallbackUri { get; set; }
        [DataMember] public string NameSpace { get; set; }
        [DataMember] public string HostName { get; set; }

        public IHostingPeer Proxy { get; set; }
    }

    [ServiceContract]
    interface ITargetsPeer
    {
        [OperationContract(IsOneWay = true)]
        void SubscribeHosting(HostingInfo info);

        [OperationContract(IsOneWay = true)]
        void CancelHosting(string hostingTag);
    }
}