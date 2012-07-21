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
    interface IVimpl : IVcall, IVhost
    { }

    class Vimpl : IVimpl
    {
        #region members

        private Action _target;

        #endregion

        #region construction

        public static IVimpl New()
        {
            return
                new Vimpl();
        }

        private Vimpl()
        { }

        #endregion

        #region IVcall

        void IVcall.Call(Expression<Action> vcall)
        {
            _target();
        }

        R IVcall.Call<R>(Expression<Func<R>> vcall)
        {
            throw new NotImplementedException();
        }

        void IVcall.Call(string methodName, params object[] args)
        {
            throw new NotImplementedException();
        }

        R IVcall.Call<R>(string functionName, params object[] args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVhost

        void IVhost.AddTarget(Action target)
        {
            _target = target;
        }

        void IVhost.AddTarget<R>(Func<R> target)
        {
            throw new NotImplementedException();
        }

        void IVhost.AddTarget<T1, T2>(Action<T1, T2> target)
        {
            throw new NotImplementedException();
        }

        void IVhost.AddTarget<T1, R>(Func<T1, R> target)
        {
            throw new NotImplementedException();
        }

        void IVhost.AddTarget<T1, T2, R>(Func<T1, T2, R> target)
        {
            throw new NotImplementedException();
        }

        void IVhost.AddTarget<T1, T2>(string targetName, Action<T1, T2> target)
        {
            throw new NotImplementedException();
        }

        R IVhost.AddTarget<T1, T2, R>(string targetName, Func<T1, T2, R> target)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}