using System;

namespace l4p.VcallModel.Core
{
    class Janitor : IRevertable
    {
        #region members
        #endregion

        #region api

        public void Add(Action action)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IRevertable

        void IRevertable.Revert()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}