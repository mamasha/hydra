/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;

namespace l4p.VcallModel
{
    public interface IVhosting : ICommNode
    {
        /// <summary>
        /// Set method to be hosted ...</summary>
        /// <param name="target">Method method group</param>
        void AddTarget(Action target);

        /// <summary>
        /// Set function to be hosted ...</summary>
        /// <typeparam name="R">Function return type</typeparam>
        /// <param name="target">Function method group</param>
        void AddTarget<R>(Func<R> target);

        void AddTarget<T1, T2>(string targetName, Action<T1, T2> target);
        R AddTarget<T1, T2, R>(string targetName, Func<T1, T2, R> target);

        void AddTarget<T1, T2>(Action<T1, T2> target);

        void AddTarget<T1, R>(Func<T1, R> target);
        void AddTarget<T1, T2, R>(Func<T1, T2, R> target);

        /// <summary>
        /// Close all underlaying services of this hosting instance </summary>
        /// <remarks>Calling any method after Close() is called will lead to unpredictable results</remarks>
        void Close();
    }
}