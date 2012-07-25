/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

namespace l4p.VcallModel.Helpers
{
    static class LoggerHelpers
    {
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
    }
}