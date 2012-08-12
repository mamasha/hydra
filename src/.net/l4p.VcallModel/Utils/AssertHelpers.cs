using System;
using System.Diagnostics;
using l4p.VcallModel.Core;

namespace l4p.VcallModel.Utils
{
    class AssertionException : VcallModelException
    {
        public AssertionException() { }
        public AssertionException(string message) : base(message) { }
        public AssertionException(string message, Exception inner) : base(message, inner) { }
    }

    static class AssertHelpers
	{
        public static void Assert<TException>(this IHelpers Helpers, bool expr, ILogger log)
            where TException : Exception, new()
        {
            if (expr)
                return;

            const string assertMsg = "Assertion expression is false";

            if (Debugger.IsAttached)
                Debug.Assert(false, assertMsg);

            throw
                Helpers.MakeNew<TException>(null, log, assertMsg);
        }

        public static void Assert(this IHelpers Helpers, bool expr, ILogger log)
        {
            Helpers.Assert<AssertionException>(expr, log);
        }

        public static void Assert<TException>(this IHelpers Helpers, bool expr, ILogger log, string format, params object[] args)
            where TException : Exception, new()
        {
            if (expr)
                return;

            string assertMsg = format;

            try
            {
                assertMsg = String.Format(format, args);
            }
            catch { }

            if (Debugger.IsAttached)
                Debug.Assert(false, assertMsg);

            throw 
                Helpers.MakeNew<TException>(null, log, assertMsg);
        }

        public static void Assert(this IHelpers Helpers, bool expr, ILogger log, string format, params object[] args)
        {
            Helpers.Assert<AssertionException>(expr, log, format, args);
        }
    }
}
