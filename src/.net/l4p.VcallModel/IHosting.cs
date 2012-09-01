/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;

namespace l4p.VcallModel
{
    public interface IHosting : ICommNode
    {
        /// <summary>
        /// Set method to be hosted ...</summary>
        /// <param name="action">Method method group</param>
        void Host(Action action);

        /// <summary>
        /// Set function to be hosted ...</summary>
        /// <typeparam name="R">Function return type</typeparam>
        /// <param name="action">Function method group</param>
        void Host<R>(Func<R> func);

        void Host<T1, T2>(string actionName, Action<T1, T2> action);
        R Host<T1, T2, R>(string funcName, Func<T1, T2, R> func);

        void Host<T1, T2>(Action<T1, T2> action);

        void Host<T1, R>(Func<T1, R> func);
        void Host<T1, T2, R>(Func<T1, T2, R> func);

        /// <summary>
        /// Actual URI this hosting is listening on</summary>
        string ListeningUri { get; }
    }
}