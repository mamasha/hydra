/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Text;

namespace l4p.VcallModel.Helpers
{
    static class ExceptionHelpers
    {
        #region private

        private static Exception make_exception(Type type, string errMsg, Exception innerException)
        {
            var args = innerException == null ?
                new object[] {errMsg} :
                new object[] {errMsg, innerException};

            try
            {
                object ex = 
                    Activator.CreateInstance(type, args);

                return 
                    (Exception) ex;
            }
            catch
            {}

            return
                new ApplicationException(errMsg, innerException);
        }

		private static string build_err_msg(string format, params object[] args)
		{
			string errMsg;

			try
			{
				errMsg = String.Format(format, args);
			}
			catch
			{
				errMsg = format;
			}

			return errMsg;
		}

        private static void build_detaild_message(Exception ex, string errMsg, StringBuilder sb)
        {
            sb
                .Append(errMsg);

            while (ex != null)
            {
                sb
                    .AppendLine()
                    .AppendFormat("   -----> {0}: {1}", ex.GetType().FullName, ex.Message);

                ex = ex.InnerException;
            }
        }

        private static void build_detailed_stack_trace(Exception ex, StringBuilder sb)
        {
            if (ex == null)
            {
                return;
            }

            build_detailed_stack_trace(ex.InnerException, sb);

            if (ex.InnerException != null)
            {
                sb
                    .AppendLine()
                    .AppendFormat("   ----- {0}: {1} -----", ex.GetType().FullName, ex.Message);
            }

            string stackTrace = ex.StackTrace;

            sb
                .AppendLine()
                .Append(stackTrace);
        }

        #endregion

        #region API

        public static string NameAndMessage(this Exception ex)
        {
            return
                String.Format("{0}: {1}", ex.GetType().FullName, ex.Message);
        }

        public static void TryCatch(this IHelpers Helpers, Action action, Action<Exception> handler)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Helpers._log.Error("{0}: {1}", ex.GetType().Name, ex.Message);
				handler(ex);
				throw;
			}
		}

		public static R TryCatch<R>(this IHelpers Helpers, Func<R> func, Action<Exception> handler)
		{
			try
			{
				return func();
			}
			catch (Exception ex)
			{
                Helpers._log.Error("{0}: {1}", ex.GetType().Name, ex.Message);
				handler(ex);
				throw;
			}
		}

		public static void ThrowNew<E>(this IHelpers Helpers, Exception inner, string format, params object[] args)
			where E : Exception, new()
		{
			string ename = typeof(E).Name;
			string errMsg = build_err_msg(format, args);

            Helpers._log.Warn("{0}: {1}", ename, errMsg);

		    throw
				make_exception(typeof(E), errMsg, inner);
		}

        public static Exception MakeNew<E>(this IHelpers Helpers, Exception inner, string format, params object[] args)
            where E : Exception, new()
        {
            string ename = typeof(E).Name;
            string errMsg = build_err_msg(format, args);

            Helpers._log.Warn("{0}: {1}", ename, errMsg);

            return
                make_exception(typeof(E), errMsg, inner);
        }

        public static string GetDetailedMessage(this Exception ex)
        {
            var sb = new StringBuilder();
            build_detaild_message(ex, ex.Message, sb);

            return sb.ToString();
        }

        public static string GetDetailedMessage(this Exception ex, string errMsg)
        {
            var sb = new StringBuilder();
            build_detaild_message(ex, errMsg, sb);

            return sb.ToString();
        }

        public static string GetDetailedStackTrace(this Exception ex)
        {
            var sb = new StringBuilder();

            build_detaild_message(ex, ex.Message, sb);
            build_detailed_stack_trace(ex, sb);

            return sb.ToString();
        }

        public static string GetDetailedStackTrace(this Exception ex, string format, params object[] args)
        {
            string errMsg = format;

            try
            {
                errMsg = String.Format(format, args);
            }
            catch
            { }

            var sb = new StringBuilder();

            build_detaild_message(ex, errMsg, sb);
            build_detailed_stack_trace(ex, sb);

            return sb.ToString();
        }

        public static bool IsConsequenceOf<TException>(this Exception ex)
            where TException : Exception
        {
            var exOfInterest = typeof(TException);

            while (ex != null)
            {
                if (exOfInterest.IsAssignableFrom(ex.GetType()))
                    return true;

                ex = ex.InnerException;
            }

            return false;
        }

        public static bool IsConsequenceOf(this Exception ex, Type exceptionType)
        {
            var exOfInterest = exceptionType;

            while (ex != null)
            {
                if (exOfInterest.IsAssignableFrom(ex.GetType()))
                    return true;

                ex = ex.InnerException;
            }

            return false;
        }

        public static bool IsConsequenceOf(this Exception ex, string subString)
        {
            while (ex != null)
            {
                if (ex.Message.Contains(subString))
                    return true;

                ex = ex.InnerException;
            }

            return false;
        }

        #endregion
    }
}