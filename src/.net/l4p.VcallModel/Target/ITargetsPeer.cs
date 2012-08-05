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
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Target
{
    class TargetsPeerException : Exception
    {
        public TargetsPeerException() { }
        public TargetsPeerException(string message) : base(message) { }
        public TargetsPeerException(string message, Exception inner) : base(message, inner) { }
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
    interface ITargetsPeer
    {
        [OperationContract(IsOneWay = true)]
        void RegisterHosting(HostingInfo info);

        [OperationContract(IsOneWay = true)]
        void CancelHosting(string hostingTag);
    }

    class TargetsPeerProxy : ClientBase<ITargetsPeer>, ITargetsPeer
    {
        public static ITargetsPeer New(string uri)
        {
            return
                new TargetsPeerProxy(uri);
        }

        private TargetsPeerProxy(String uri) :
            base(new TcpStreamBindng(), new EndpointAddress(uri))
        { }

        void ITargetsPeer.RegisterHosting(HostingInfo info) { Channel.RegisterHosting(info); }
        void ITargetsPeer.CancelHosting(string hostingTag) { Channel.CancelHosting(hostingTag); }
    }
}