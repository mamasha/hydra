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
        [DataMember] public int Vcall_Event_NewTargets { get; set; }
        [DataMember] public int Vcall_Event_CloseCommNode { get; set; }

        [DataMember] public int Vcall_Error_AddressInUse { get; set; }
        [DataMember] public int Vcall_Error_NewHostingFailed { get; set; }
        [DataMember] public int Vcall_Error_NewTargetsFailed { get; set; }
        [DataMember] public int Vcall_Error_CloseCommNode { get; set; }
        [DataMember] public int Vcall_Error_InternalFailure { get; set; }

        [DataMember] public int Vcall_State_DurableOperations { get; set; }
        [DataMember] public int Vcall_State_ActiveNodes { get; set; }

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

        [DataMember] public int Targets_Event_IsStarted { get; set; }
        [DataMember] public int Targets_Event_IsStopped { get; set; }
        [DataMember] public int Targets_Event_IsAlreadyStopped { get; set; }
        [DataMember] public int Targets_Event_AliveHosting { get; set; }
        [DataMember] public int Targets_Event_DeadHosting { get; set; }
        [DataMember] public int Targets_Event_NewHosing { get; set; }
        [DataMember] public int Targets_Event_CanceledHosting { get; set; }
        [DataMember] public int Targets_Event_KnownHosing { get; set; }
        [DataMember] public int Targets_Event_UnknownHosing { get; set; }
        [DataMember] public int Targets_Event_SubscribedToHosting { get; set; }
        [DataMember] public int Targets_Event_NewWcfChannel { get; set; }

        [DataMember] public int Targets_Error_SubscribeToHosting { get; set; }
        [DataMember] public int Targets_Error_HostingCalls { get; set; }

        [DataMember] public int Targets_State_AliveHostings { get; set; }

        //----------------------------------------------------------//

        [DataMember] public int Hosting_Event_IsStarted { get; set; }
        [DataMember] public int Hosting_Event_IsStopped { get; set; }
        [DataMember] public int Hosting_Event_IsAlreadyStopped { get; set; }
        [DataMember] public int Hosting_Event_AliveTargets { get; set; }
        [DataMember] public int Hosting_Event_DeadTargets { get; set; }
        [DataMember] public int Hosting_Event_NewTargets { get; set; }
        [DataMember] public int Hosting_Event_CanceledTargets { get; set; }
        [DataMember] public int Hosting_Event_KnownTargets { get; set; }
        [DataMember] public int Hosting_Event_UnknownTargets { get; set; }
        [DataMember] public int Hosting_Event_SubscribedToTargets { get; set; }
        [DataMember] public int Hosting_Event_NewWcfChannel { get; set; }

        [DataMember] public int Hosting_Error_SubscribeToTargets { get; set; }
        [DataMember] public int Hosting_Error_TargetsCalls { get; set; }

        [DataMember] public int Hosting_State_AliveTargets { get; set; }

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