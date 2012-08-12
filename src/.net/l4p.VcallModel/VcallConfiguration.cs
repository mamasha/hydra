using System;

namespace l4p.VcallModel
{
    public class VcallConfiguration
    {
        public string ResolvingKey { get; set; }
        public string DiscoveryScopePattern { get; set; }

        public int? Port { get; set; }
        public string CallbackUriPattern { get; set; }

        public int AddressInUseRetries { get; set; }
        public Timeouts Timeouts { get; set; }

        public VcallConfiguration()
        {
            ResolvingKey = Guid.NewGuid().ToString("B");
            DiscoveryScopePattern = "udp://l4p.vcallmodel/discovery/{0}/";
            CallbackUriPattern = "net.tcp://{0}:{1}/{2}/{3}/";
            AddressInUseRetries = 3;
            Timeouts = new Timeouts();
        }

        public VcallConfiguration Clone()
        {
            return
                (VcallConfiguration) this.MemberwiseClone();
        }
    }

    public class Timeouts
    {
        public int ByeMessageGap { get; set; }
        public int HelloMessageGap { get; set; }
        public int DiscoveryOpening { get; set; }
        public int DiscoveryClosing { get; set; }
        public int HostingOpening { get; set; }
        public int HostingClosing { get; set; }
        public int TargetOpening { get; set; }

        public int DurableQueue_NoDurablesIdle { get; set; }
        public int ActiveThread_FailureTimeout { get; set; }

        public Timeouts()
        {
            const int OpenWcfHostTimeout = 5000;
            const int CloseWcfHostTimeout = 1000;

            ByeMessageGap = 10000;
            HelloMessageGap = 3000;
            DiscoveryOpening = OpenWcfHostTimeout;
            DiscoveryClosing = CloseWcfHostTimeout;
            HostingOpening = OpenWcfHostTimeout;
            HostingClosing = CloseWcfHostTimeout;
            TargetOpening = OpenWcfHostTimeout;

            DurableQueue_NoDurablesIdle = 1000;
            ActiveThread_FailureTimeout = 10000;
        }
    }

}