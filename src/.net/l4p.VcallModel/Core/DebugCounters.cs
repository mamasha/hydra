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
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Core
{
    [DataContract]
    public class DebugCounters
    {
        [DataMember] public int HelloMsgsSent { get; set; }
        [DataMember] public int HelloMsgsRecieved { get; set; }
        [DataMember] public int HelloMsgsFiltered { get; set; }
        [DataMember] public int MyHelloMsgsReceived { get; set; }
        [DataMember] public int OtherHelloMsgsReceived { get; set; }
        [DataMember] public int HelloNotificationsProduced { get; set; }
        [DataMember] public int ByeNotificationsProduced { get; set; }

        [DataMember] public int HostingsOpened { get; set; }
        [DataMember] public int HostingsClosed { get; set; }
        [DataMember] public int TargetsOpened { get; set; }
        [DataMember] public int TargetsClosed { get; set; }

        [DataMember] public int Target_ConnectedHosts { get; set; }
        [DataMember] public int Hosting_ConnectedTargets { get; set; }
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