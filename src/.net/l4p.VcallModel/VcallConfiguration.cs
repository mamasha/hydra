using System;

namespace l4p.VcallModel
{
    public class VcallConfiguration
    {
        public string ResolvingKey { get; set; }
        public string DiscoveryScopePattern { get; set; }

        public Timeouts Timeouts { get; set; }

        public VcallConfiguration()
        {
            ResolvingKey = Guid.NewGuid().ToString("B");
            DiscoveryScopePattern = "udp://l4p.vcallmodel/discovery/{0}/";
            Timeouts = new Timeouts();
        }
    }

    public class Timeouts
    {
        public int ServiceAliveTimeSpan { get; set; }
        public int DiscoveryUpdatePeriod { get; set; }
        public int DiscoveryOpening { get; set; }
        public int DiscoveryClosing { get; set; }

        public Timeouts()
        {
            ServiceAliveTimeSpan = 10000;
            DiscoveryUpdatePeriod = 3000;
            DiscoveryOpening = 5000;
            DiscoveryClosing = 1000;
        }
    }

}