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

        [DataMember] public int Vcall_Event_NewHosting { get; set; }
        [DataMember] public int Vcall_Event_CloseHosting { get; set; }
        [DataMember] public int Vcall_Event_NewTargets { get; set; }
        [DataMember] public int Vcall_Event_CloseTargets { get; set; }

        [DataMember] public int Vcall_State_DurableOperations { get; set; }

        [DataMember] public int Targets_Event_HostIsAlive { get; set; }
        [DataMember] public int Targets_Event_HostIsDead { get; set; }
        [DataMember] public int Targets_Event_SubscribeHosing { get; set; }
        [DataMember] public int Targets_Event_TargetsCanceled { get; set; }
        [DataMember] public int Targets_Event_AlreadyHereHosting { get; set; }
        [DataMember] public int Targets_Event_SubscribeSelfToHosting { get; set; }

        [DataMember] public int Hosting_Event_AliveHosts { get; set; }
        [DataMember] public int Hosting_Event_DeadHosts { get; set; }
        [DataMember] public int Hosting_Event_SubscribeTargets { get; set; }
        [DataMember] public int Hosting_Event_AlreadyHereTargets { get; set; }
        [DataMember] public int Hosting_Event_SubscribeSelfToTargets { get; set; }

        [DataMember] public int Targets_Error_SubscribeToHosting { get; set; }

        [DataMember] public int Hosting_Error_SubscribeSelfToTargets { get; set; }

        public static DebugCounters AccumulateAll(params DebugCounters[] all)
        {
            var sum = new DebugCounters();
            Array.ForEach(all, counters => sum.Accumulate(counters));
            return sum;
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
                    .AppendFormat("  {0,-32} {1}", name, value);
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