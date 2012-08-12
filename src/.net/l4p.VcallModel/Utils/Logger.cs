/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace l4p.VcallModel.Utils
{
    public interface ILogger
    {
        bool TraceIsOff { get; }

        void Error(string format, params object[] args);
        void Error(Exception ex, string format, params object[] args);
        void Warn(string format, params object[] args);
        void Warn(Exception ex, string format, params object[] args);
        void Info(string format, params object[] args);
        void Trace(string format, params object[] args);
    }

    public enum LogPriority
    {
        None = 0,
        Fatal = 1,
        Error = 2,
        Warn = 3,
        Info = 4,
        Trace = 5
    }

    public class Logger : ILogger
    {
        #region members

        private static LogPriority _priority = LogPriority.Trace;

        private string _name;

        #endregion

        #region construction

        public static ILogger New(string name)
        {
            return
                new Logger(name);
        }

        public static ILogger New<T>()
        {
            return
                new Logger(typeof(T).Name);
        }

        private Logger(string name)
        {
            _name = String.Format("[{0}]", name);
        }

        #endregion

        #region private

        private static string build_prefix(string priorityLetter)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
            int pid = Process.GetCurrentProcess().Id;
            int tid = Thread.CurrentThread.ManagedThreadId;

            return
                String.Format("{0} {1}.thread.{2} {3} ", now, pid, tid, priorityLetter);
        }

        private static string build_user_message(string format, params object[] args)
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

        private void send_message(string msg)
        {
//            Console.WriteLine(msg);
            Trace.WriteLine(msg, _name);
        }

        #endregion

        #region ILogger

        public bool TraceIsOff
        {
            get { return false; }
        }

        void ILogger.Error(string format, params object[] args)
        {
            if (_priority < LogPriority.Error)
                return;

            string msg = build_prefix("E") + build_user_message(format, args);
            send_message(msg);
        }

        void ILogger.Error(Exception ex, string format, params object[] args)
        {
            if (_priority < LogPriority.Error)
                return;

            var sbMsg = new StringBuilder();

            sbMsg
                .StartWithNewLine()
                .Append(build_prefix("E") + build_user_message(format, args))
                .AppendLine()
                .Append(ex.GetDetailedMessage());

            send_message(sbMsg.ToString());
        }

        void ILogger.Warn(string format, params object[] args)
        {
            if (_priority < LogPriority.Warn)
                return;

            string msg = build_prefix("W") + build_user_message(format, args);
            send_message(msg);
        }

        void ILogger.Warn(Exception ex, string format, params object[] args)
        {
            if (_priority < LogPriority.Warn)
                return;

            var sbMsg = new StringBuilder();

            sbMsg
                .StartWithNewLine()
                .Append(build_prefix("W") + build_user_message(format, args))
                .AppendLine()
                .Append(ex.GetDetailedMessage());

            send_message(sbMsg.ToString());
        }

        void ILogger.Info(string format, params object[] args)
        {
            if (_priority < LogPriority.Info)
                return;

            string msg = build_prefix("I") + build_user_message(format, args);

            send_message(msg);
        }

        void ILogger.Trace(string format, params object[] args)
        {
            if (_priority < LogPriority.Trace)
                return;

            string msg = build_prefix("T") + build_user_message(format, args);

            send_message(msg);
        }

        #endregion
    }
}