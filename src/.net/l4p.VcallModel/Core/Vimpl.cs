/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Linq.Expressions;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel.Core
{
    class ResolutionDomain
    {}

    interface IVimpl : IVtarget, IVhosting
    { }

    class Vimpl : IVimpl
    {
        #region members

        private static readonly ILogger _log = Logger.New("Vimpl");
        private static readonly IHelpers Helpers = Utils.New(_log);

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

        void IVtarget.Call(Expression<Action> vcall)
        {
            _target();
        }

        R IVtarget.Call<R>(Expression<Func<R>> vcall)
        {
            throw new NotImplementedException();
        }

        void IVtarget.Call(string methodName, params object[] args)
        {
            throw 
                Helpers.MakeNew<VcallException>(null, "'{0}' There is no registered targets for subject '{1}'", "resolving key", methodName);

            throw new NotImplementedException();
        }

        R IVtarget.Call<R>(string functionName, params object[] args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVhost

        void IVhosting.AddTarget(Action target)
        {
            _target = target;
        }

        void IVhosting.AddTarget<R>(Func<R> target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2>(Action<T1, T2> target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.AddTarget<T1, R>(Func<T1, R> target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2, R>(Func<T1, T2, R> target)
        {
            throw new NotImplementedException();
        }

        void IVhosting.AddTarget<T1, T2>(string targetName, Action<T1, T2> target)
        {
            throw new NotImplementedException();
        }

        R IVhosting.AddTarget<T1, T2, R>(string targetName, Func<T1, T2, R> target)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}