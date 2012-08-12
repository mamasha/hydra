/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Net.NetworkInformation;
using System.Text;

namespace l4p.VcallModel.Utils
{
	static class MiscellaneousHelpers
	{
        public static StringBuilder StartWithNewLine(this StringBuilder sb)
        {
            if (sb.Length == 0)
                return sb;

            char lastChar = sb[sb.Length - 1];

            if (lastChar == '\n' || lastChar == '\r')
                return sb;

            sb.AppendLine();
            return sb;
        }

        public static string SafeFormat(this IHelpers Helpers, string format, params object[] args)
        {
            string str = format;

            try
            {
                str = string.Format(format, args);
            }
            catch
            { }

            return str;
        }

        public static string GetLocalhostFqdn(this IHelpers Helpers)
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            return 
                String.Format("{0}.{1}", ipProperties.HostName, ipProperties.DomainName);
        }
    }
}
