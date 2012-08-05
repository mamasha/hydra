using System;
using l4p.VcallModel.Core;

namespace l4p.VcallModel.Discovery
{
    class Self
    {
        public HostResolverConfiguration config { get; set; }
        public Uri resolvingScope { get; set; }

        public IRepository repo { get; set; }

        public IWcfDiscovery wcf { get; set; }
        public IResolvingThread thread { get; set; }
    }
}