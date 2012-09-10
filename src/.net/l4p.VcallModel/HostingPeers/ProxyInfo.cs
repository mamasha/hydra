using System.Runtime.Serialization;
using l4p.VcallModel.ProxyPeers;

namespace l4p.VcallModel.HostingPeers
{
    [DataContract]
    class ProxyInfo
    {
        [DataMember] public string Tag { get; set; }
        [DataMember] public string ListeningUri { get; set; }
        [DataMember] public string NameSpace { get; set; }
        [DataMember] public string HostName { get; set; }

        public IProxyPeer Proxy { get; set; }
    }
}