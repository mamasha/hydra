/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;

namespace l4p.VcallModel
{
    public interface IVtarget : IDisposable
    {
        /// <summary>
        /// Call a method out there ...</summary>
        /// <param name="vcall">A closure with input parameters ...</param>
        void Call(Expression<Action> vcall);

        /// <summary>
        /// Call a function out there ...</summary>
        /// <typeparam name="R">Returning type</typeparam>
        /// <param name="vcall">A closure with input parameters</param>
        /// <returns>Function result value</returns>
        R Call<R>(Expression<Func<R>> vcall);

        /// <summary>
        /// Call a method out there, resolving its name with explicit parameter </summary>
        /// <param name="methodName">The method name to be called out there</param>
        /// <param name="args">Arguments of a call</param>
        void Call(string methodName, params object[] args);

        /// <summary>
        /// Call a function out there, resolving its name with explicit parameter </summary>
        /// <typeparam name="R">Function return type</typeparam>
        /// <param name="functionName">The function name to be called out there</param>
        /// <param name="args">Arguments of a call</param>
        /// <returns>Function return value</returns>
        R Call<R>(string functionName, params object[] args);

        /// <summary>
        /// Close all underlaying services of this target instance </summary>
        /// <remarks>Calling any method after Close() is called will lead to unpredictable results</remarks>
        void Close();
    }
}