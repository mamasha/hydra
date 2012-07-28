using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace l4p.VcallModel.Helpers
{
    static class AssertHelpers
	{
        public static void Assert<TException>(this IHelpers Helpers, bool expr)
            where TException : Exception, new()
        {
            if (expr)
                return;

            const string assertMsg = "Assertion expression is false";

            if (Debugger.IsAttached)
                Debug.Assert(false, assertMsg);

            throw
                Helpers.MakeNew<TException>(null, assertMsg);
        }

        public static void Assert(this IHelpers Helpers, bool expr)
        {
            Helpers.Assert<InternalException>(expr);
        }

        public static void Assert<TException>(this IHelpers Helpers, bool expr, string format, params object[] args)
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
                Helpers.MakeNew<TException>(null, assertMsg);
        }

        public static void Assert(this IHelpers Helpers, bool expr, string format, params object[] args)
        {
            Helpers.Assert<InternalException>(expr, format, args);
        }
    }
}
