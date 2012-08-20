/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using l4p.VcallModel.Target;
using l4p.VcallModel.Target.Proxies;

namespace l4p.VcallModel.Hosting
{
    class HostingPeerException : VcallModelException
    {
        public HostingPeerException() { }
        public HostingPeerException(string message) : base(message) { }
        public HostingPeerException(string message, Exception inner) : base(message, inner) { }
    }

    [DataContract]
    class TargetsInfo
    {
        [DataMember] public string Tag { get; set; }
        [DataMember] public string ListeningUri { get; set; }
        [DataMember] public string NameSpace { get; set; }
        [DataMember] public string HostName { get; set; }

        public ITargetsPeer Proxy { get; set; }
    }

    [ServiceContract]
    interface IHostingPeer
    {
        [OperationContract(IsOneWay = true)]
        void SubscribeTargets(TargetsInfo info);

        [OperationContract(IsOneWay = true)]
        void CancelTargets(string targetsTag);
    }
}