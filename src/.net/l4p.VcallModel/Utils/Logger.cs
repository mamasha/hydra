/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using l4p.VcallModel.Configuration;

namespace l4p.VcallModel.Utils
{
    interface ILogger
    {
        bool TraceIsOff { get; }

        void Error(string format, params object[] args);
        void Error(Exception ex, string format, params object[] args);
        void Warn(string format, params object[] args);
        void Warn(Exception ex, string format, params object[] args);
        void Info(string format, params object[] args);
        void Trace(string format, params object[] args);
    }

    class Logger : ILogger
    {
        #region members

        private static LoggingConfiguration _config = new LoggingConfiguration();
        private string _name;

        #endregion

        #region construction

        public static LoggingConfiguration Config
        {
            get { return _config; }
            set
            {
                ensure_folder_exists(value.ToFile);
                _config = value;
            }
        }

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
                String.Format("{0} {1}.thread.{2:000}  {3} ", now, pid, tid, priorityLetter);
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

        private static void ensure_folder_exists(string logPath)
        {
            if (logPath == null)
                return;

            string path = Path.GetDirectoryName(logPath);
            Directory.CreateDirectory(path);
        }

        private static void send_to_file(string msg, string logPath, int maxRetires)
        {
            var lines = new[] {msg};
            int retries = 0;
            int snoozeFor = 1;

            for(;;)
            {
                try
                {
                    File.AppendAllLines(logPath, lines);
                    return;
                }
                catch (Exception)
                {
                    if (retries++ > maxRetires)
                        throw;
                }

                snoozeFor = Math.Min(snoozeFor*2, 200);
                Thread.Sleep(snoozeFor);
            }
        }

        private void send_message(string msg)
        {
            try
            {
                if (_config.ToTrace)
                    Trace.WriteLine(msg, _name);

                if (_config.ToConsole)
                    Console.WriteLine(msg);

                if (_config.ToMethod != null)
                    _config.ToMethod(msg);

                if (_config.ToFile != null)
                    send_to_file(msg, _config.ToFile, _config.WriteToFileRetires);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to log a message; " + ex.Message);
            }
        }

        #endregion

        #region ILogger

        public bool TraceIsOff
        {
            get { return _config.Level < LogLevel.Trace; }
        }

        void ILogger.Error(string format, params object[] args)
        {
            const LogLevel msgLevel = LogLevel.Error;

            if (msgLevel > _config.Level)
                return;

            string msg = build_prefix("E") + build_user_message(format, args);
            send_message(msg);
        }

        void ILogger.Error(Exception ex, string format, params object[] args)
        {
            const LogLevel msgLevel = LogLevel.Error;

            if (msgLevel > _config.Level)
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
            const LogLevel msgLevel = LogLevel.Warn;

            if (msgLevel > _config.Level)
                return;

            string msg = build_prefix("W") + build_user_message(format, args);
            send_message(msg);
        }

        void ILogger.Warn(Exception ex, string format, params object[] args)
        {
            const LogLevel msgLevel = LogLevel.Warn;

            if (msgLevel > _config.Level)
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
            const LogLevel msgLevel = LogLevel.Info;

            if (msgLevel > _config.Level)
                return;

            string msg = build_prefix("I") + build_user_message(format, args);

            send_message(msg);
        }

        void ILogger.Trace(string format, params object[] args)
        {
            const LogLevel msgLevel = LogLevel.Trace;

            if (msgLevel > _config.Level)
                return;

            string msg = build_prefix("T") + build_user_message(format, args);

            send_message(msg);
        }

        #endregion
    }
}