/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Reflection;

namespace l4p.VcallModel.Utils
{
    public class InternalAccessException : Exception
    {
        public InternalAccessException() { }
        public InternalAccessException(string message) : base(message) { }
        public InternalAccessException(string message, Exception inner) : base(message, inner) { }
    }

    public class Internal
    {
        internal Internal()
        { }
    }

    static class InternalAccess
    {
        public static void Check(Internal access)
        {
            if (access == null)
            {
                throw new InternalAccessException(string.Format(
                    "method should be called by '{0}' assembly only", Assembly.GetEntryAssembly().GetName()));
            }
        }
    }
}