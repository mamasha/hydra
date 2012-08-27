using System;
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
        public LoggingConfiguration Logging { get; set; }

        public VcallConfiguration()
        {
            ResolvingKey = MiscellaneousHelpers.RandomName8(null);
            DiscoveryScopePattern = "udp://l4p.vcallmodel/discovery/{0}/";
            CallbackUriPattern = "net.tcp://{0}:{1}/{2}/{3}/";
            TargetsRole = "targets";
            HostingRole = "hosting";
            AddressInUseRetries = 3;
            Timeouts = new Timeouts();
            Logging = new LoggingConfiguration();
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
        public int WcfHostClosing { get; set; }
        public int TargetsHostingSubscriptionRetry { get; set; }

        public int DurableQueue_NoDurablesIdle { get; set; }
        public int ActiveThread_Start { get; set; }
        public int ActiveThread_Stop { get; set; }
        public int ActiveThread_DurableTimeToLive { get; set; }

        public Timeouts()
        {
            ByeMessageGap = 10*1000;
            HelloMessageGap = 3*1000;
            DiscoveryOpening = 5*1000;
            DiscoveryClosing = 10*1000;
            HostingOpening = 5*1000;
            NodeClosing = 10*1000;
            WcfHostClosing = 5*1000;
            TargetOpening = 5*1000;
            TargetsHostingSubscriptionRetry = 1*1000;

            DurableQueue_NoDurablesIdle = 1*1000;
            ActiveThread_Start = 5*1000;
            ActiveThread_Stop = 2*1000;
            ActiveThread_DurableTimeToLive = 60*1000;
        }
    }

    public enum LogLevel
    {
        Off, Error, Warn, Info, Trace
    }

    public class LoggingConfiguration
    {
        public LogLevel Level { get; set; }
        public string ToFile { get; set; }
        public int WriteToFileRetires { get; set; }
        public bool ToTrace { get; set; }
        public bool ToConsole { get; set; }
        public Action<string> ToMethod { get; set; }

        public LoggingConfiguration()
        {
            Level = LogLevel.Trace;
            ToFile = null;
            WriteToFileRetires = 21;
            ToTrace = true;
            ToConsole = false;
            ToMethod = null;
        }
    }
}