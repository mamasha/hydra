using System;
using System.Collections.Generic;
using System.Threading;

namespace l4p.VcallModel.Utils
{
    class ContextException : VcallModelException
    {
        public ContextException() { }
        public ContextException(string message) : base(message) { }
        public ContextException(string message, Exception inner) : base(message, inner) { }
    }

    interface ITypedStacks
    {
        T Get<T>();
        void Push<T>(T value);
        void Pop<T>();
    }

    class TypedStacks : ITypedStacks
    {
        #region members

        private Dictionary<Type, Stack<object>> _db;

        #endregion

        #region construction

        public static ITypedStacks New()
        {
            return
                new TypedStacks();
        }

        private TypedStacks()
        {
            _db = new Dictionary<Type, Stack<object>>();
        }

        #endregion

        #region Implementation of ITypedStacks

        T ITypedStacks.Get<T>()
        {
            Type type = typeof(T);
            Stack<object> stack;

            switch (true)
            {
                case true:

                if (_db.TryGetValue(type, out stack) == false)
                    break;

                if (stack.Count == 0)
                    break;

                return
                    (T) stack.Peek();
            }

            throw
                new ContextException(String.Format("The value of type '{0}' has not been set", type.Name));
        }

        void ITypedStacks.Push<T>(T value)
        {
            Type type = typeof(T);
            Stack<object> stack;

            if (_db.TryGetValue(type, out stack) == false)
            {
                stack = new Stack<object>();
                _db.Add(type, stack);
            }

            stack.Push(value);
        }

        void ITypedStacks.Pop<T>()
        {
            Type type = typeof(T);
            Stack<object> stack;

            switch (true)
            {
                case true:

                if (_db.TryGetValue(type, out stack) == false)
                    break;

                if (stack.Count == 0)
                    break;

                stack.Pop();
                return;
            }

            throw
                new ContextException(String.Format("No value of type '{0}' can be popped", type.Name));
        }

        #endregion
    }

    static class Context
    {
        #region helpers

        private class Popper<T> : IDisposable
        {
            private ITypedStacks _typedStacks;

            public Popper(ITypedStacks typedStacks)
            {
                _typedStacks = typedStacks;
            }

            void IDisposable.Dispose()
            {
                _typedStacks.Pop<T>();
            }
        }

        #endregion

        #region members

        private static ThreadLocal<ITypedStacks> _typedStacks;

        #endregion

        #region construction

        static Context()
        {
            _typedStacks = new ThreadLocal<ITypedStacks>(TypedStacks.New);
        }

        #endregion

        #region api

        public static IDisposable With<T>(T value)
        {
            var typedStacks = _typedStacks.Value;
            typedStacks.Push(value);

            return
                new Popper<T>(typedStacks);
        }

        public static T Get<T>()
        {
            return
                _typedStacks.Value.Get<T>();
        }

        #endregion
    }
}