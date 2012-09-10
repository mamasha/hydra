/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Core
{
    [DataContract]
    public class DebugCounters
    {
        [DataMember] public int Vcall_Event_NewHosting { get; set; }
        [DataMember] public int Vcall_Event_NewProxy { get; set; }
        [DataMember] public int Vcall_Event_CloseCommPeer { get; set; }

        [DataMember] public int Vcall_Error_AddressInUse { get; set; }
        [DataMember] public int Vcall_Error_NewHostingFailed { get; set; }
        [DataMember] public int Vcall_Error_NewProxyFailed { get; set; }
        [DataMember] public int Vcall_Error_CloseCommPeer { get; set; }
        [DataMember] public int Vcall_Error_InternalFailure { get; set; }

        [DataMember] public int Vcall_State_DurableOperations { get; set; }
        [DataMember] public int Vcall_State_ActivePeers { get; set; }

        //----------------------------------------------------------//

        [DataMember] public int Discovery_Event_ByeNotificationsProduced { get; set; }
        [DataMember] public int Discovery_Event_HelloMsgsFiltered { get; set; }
        [DataMember] public int Discovery_Event_HelloMsgsRecieved { get; set; }
        [DataMember] public int Discovery_Event_HelloMsgsSent { get; set; }
        [DataMember] public int Discovery_Event_HelloNotificationsProduced { get; set; }
        [DataMember] public int Discovery_Event_HelloMsgIsKeepAlive { get; set; }
        [DataMember] public int Discovery_Event_MyHelloMsgsReceived { get; set; }
        [DataMember] public int Discovery_Event_OtherHelloMsgsReceived { get; set; }

        [DataMember] public int Discovery_State_ActivePublishers { get; set; }
        [DataMember] public int Discovery_State_ActiveSubscribers { get; set; }
        [DataMember] public int Discovery_State_AliveRemotePeers { get; set; }

        //----------------------------------------------------------//

        [DataMember] public int ProxyPeer_Event_IsStarted { get; set; }
        [DataMember] public int ProxyPeer_Event_IsStopped { get; set; }
        [DataMember] public int ProxyPeer_Event_IsAlreadyStopped { get; set; }
        [DataMember] public int ProxyPeer_Event_HelloFromHosting { get; set; }
        [DataMember] public int ProxyPeer_Event_ByeFromHosting { get; set; }
        [DataMember] public int ProxyPeer_Event_NewHosting { get; set; }
        [DataMember] public int ProxyPeer_Event_CanceledHosting { get; set; }
        [DataMember] public int ProxyPeer_Event_KnownHosting { get; set; }
        [DataMember] public int ProxyPeer_Event_UnknownHosting { get; set; }
        [DataMember] public int ProxyPeer_Event_SubscribedToHosting { get; set; }
        [DataMember] public int ProxyPeer_Event_NewWcfChannel { get; set; }

        [DataMember] public int ProxyPeer_Error_SubscribeToHosting { get; set; }
        [DataMember] public int ProxyPeer_Error_HostingCalls { get; set; }

        [DataMember] public int ProxyPeer_State_AliveHostings { get; set; }

        //----------------------------------------------------------//

        [DataMember] public int HostingPeer_Event_IsStarted { get; set; }
        [DataMember] public int HostingPeer_Event_IsStopped { get; set; }
        [DataMember] public int HostingPeer_Event_IsAlreadyStopped { get; set; }
        [DataMember] public int HostingPeer_Event_HelloFromProxy { get; set; }
        [DataMember] public int HostingPeer_Event_ByeFromProxy { get; set; }
        [DataMember] public int HostingPeer_Event_NewProxy { get; set; }
        [DataMember] public int HostingPeer_Event_CanceledProxy { get; set; }
        [DataMember] public int HostingPeer_Event_KnownProxy { get; set; }
        [DataMember] public int HostingPeer_Event_UnknownProxy { get; set; }
        [DataMember] public int HostingPeer_Event_SubscribedToProxy { get; set; }
        [DataMember] public int HostingPeer_Event_NewWcfChannel { get; set; }

        [DataMember] public int HostingPeer_Error_SubscribeToProxy { get; set; }
        [DataMember] public int HostingPeer_Error_ProxyCalls { get; set; }

        [DataMember] public int HostingPeer_State_AliveProxies { get; set; }

        //----------------------------------------------------------//

        [DataMember] public int InvocationBus_Event_NotMyNamespace { get; set; }

        //----------------------------------------------------------//

        [DataMember] public int Hosting_Event_NotMyNamespace { get; set; }

        //----------------------------------------------------------//

        internal DebugCounters(CountersDb cdb)
        { }

        //----------------------------------------------------------//

        public override string ToString()
        {
            return
                this.Format();
        }
    }

    public static class DebugCountersFormatter
    {
        private static readonly PropertyInfo[] _counterFields = typeof(DebugCounters).GetProperties();

        public static void Format(this DebugCounters counters, StringBuilder sb)
        {
            foreach (var field in _counterFields)
            {
                string name = field.Name;
                int value = (int) field.GetValue(counters, null);

                sb
                    .StartWithNewLine()
                    .AppendFormat("  {0,-48} {1}", name, value);
            }
        }

        public static string Format(this DebugCounters counters, string format, params object[] args)
        {
            var sb = new StringBuilder();

            sb.AppendFormat(format, args);

            counters.Format(sb);

            return 
                sb.ToString();
        }

        public static string Format(this DebugCounters counters)
        {
            var sb = new StringBuilder();

            sb.AppendLine(counters.GetType().FullName);

            counters.Format(sb);

            return
                sb.ToString();
        }

        public static void Accumulate(this DebugCounters lhsCounters, DebugCounters rhsCounters)
        {
            foreach (var field in _counterFields)
            {
                int lhs = (int) field.GetValue(lhsCounters, null);
                int rhs = (int) field.GetValue(rhsCounters, null);

                field.SetValue(lhsCounters, lhs + rhs, null);
            }
        }
    }
}