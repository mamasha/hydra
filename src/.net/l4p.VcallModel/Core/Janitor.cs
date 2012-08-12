using System;
using System.Collections.Generic;

namespace l4p.VcallModel.Core
{
    class Janitor : IRevertable
    {
        #region members

        private readonly List<Action> _actions;

        #endregion

        public Janitor()
        {
            _actions = new List<Action>();
        }

        #region api

        public void Add(Action action)
        {
            _actions.Add(action);
        }

        #endregion

        #region IRevertable

        void IRevertable.Revert()
        {
            _actions.Reverse();
            _actions.ForEach(action => action());
        }

        #endregion
    }
}