using System;
using System.IO;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Configuration
{
    public class VcallConfiguration
    {
        public string ResolvingKey { get; set; }
        public string DiscoveryScopePattern { get; set; }

        public int? Port { get; set; }
        public string CallbackUriPattern { get; set; }

        public string TargetsRole { get; set; }
        public string HostingRole { get; set; }
        public NonRegisteredCallPolicy NonRegisteredCall { get; set; }

        public int AddressInUseRetries { get; set; }
        public Timeouts Timeouts { get; set; }
        public Logging Logging { get; set; }

        public VcallConfiguration()
        {
            ResolvingKey = MiscellaneousHelpers.RandomName8(null);
            DiscoveryScopePattern = "udp://l4p.vcallmodel/discovery/{0}/";
            CallbackUriPattern = "net.tcp://{0}:{1}/{2}/{3}/";
            TargetsRole = "targets";
            HostingRole = "hosting";
            AddressInUseRetries = 3;
            Timeouts = new Timeouts();
            Logging = new Logging();
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
        public int TargetOpening { get; set; }
        public int NodeClosing { get; set; }
        public int TargetsHostingSubscription { get; set; }

        public int DurableQueue_NoDurablesIdle { get; set; }
        public int ActiveThread_FailureTimeout { get; set; }

        public Timeouts()
        {
            const int OpenWcfHostTimeout = 5000;
            const int CloseWcfHostTimeout = 10000;

            ByeMessageGap = 10000;
            HelloMessageGap = 3000;
            DiscoveryOpening = OpenWcfHostTimeout;
            DiscoveryClosing = CloseWcfHostTimeout;
            HostingOpening = OpenWcfHostTimeout;
            NodeClosing = CloseWcfHostTimeout;
            TargetOpening = OpenWcfHostTimeout;
            TargetsHostingSubscription = 1000;

            DurableQueue_NoDurablesIdle = 1000;
            ActiveThread_FailureTimeout = 60000;
        }
    }

    public enum LoggingLevel
    {
        Off, Error, Warn, Info, Trace
    }

    public class Logging
    {
        public LoggingLevel Level { get; set; }
        public string ToFile { get; set; }
        public bool ToTrace { get; set; }
        public bool ToConsole { get; set; }
        public Action<string> ToMethod { get; set; }

        public Logging()
        {
            Level = LoggingLevel.Trace;
            ToFile = null;
            ToTrace = true;
            ToConsole = false;
            ToMethod = null;
        }
    }
}