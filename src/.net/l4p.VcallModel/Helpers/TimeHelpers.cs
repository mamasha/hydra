/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;

namespace l4p.VcallModel.Helpers
{
    static class TimeHelpers
    {
        private static string make_message(string format, params object[] args)
        {
            string msg = format;

            try
            {
                msg = String.Format(format, args);
            }
            catch
            { }

            return msg;
        }

        public static void TimedAction(this IHelpers Helpers, Action action, string format, params object[] args)
        {
            try
            {
                action();
            }
            catch (TimeoutException ex)
            {
                string errMsg = make_message(format, args);

                throw
                    new TimeoutException(errMsg, ex);
            }
        }

        public static R TimedAction<R>(this IHelpers Helpers, Func<R> func, string format, params object[] args)
        {
            try
            {
                return func();
            }
            catch (TimeoutException ex)
            {
                string errMsg = make_message(format, args);

                throw
                    new TimeoutException(errMsg, ex);
            }
        }

        public static TimeSpan MakeTimeSpan(this IHelpers Helpers, int milliseconds)
        {
            return
                new TimeSpan(0, 0, 0, 0, milliseconds);
        }
    }
}