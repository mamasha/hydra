using System.Runtime.Serialization;
using l4p.VcallModel.HostingPeers;

namespace l4p.VcallModel.ProxyPeers
{
    [DataContract]
    class HostingInfo
    {
        [DataMember] public string Tag { get; set; }
        [DataMember] public string CallbackUri { get; set; }
        [DataMember] public string NameSpace { get; set; }
        [DataMember] public string HostName { get; set; }

        public IHostingPeer Proxy { get; set; }
    }
}