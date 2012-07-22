/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;

namespace l4p.VcallModel.Core
{
    interface IRepository
    {
        IResolvingDomain FindResolivingDomain(string resolvingKey);
        IResolvingDomain AddResolivingDomain(string resolvingKey, IResolvingDomain rdomain);
    }

    class Repository : IRepository
    {
        #region members
        #endregion

        #region construction
        #endregion

        #region IRepository

        IResolvingDomain IRepository.FindResolivingDomain(string resolvingKey)
        {
            throw new NotImplementedException();
        }

        IResolvingDomain IRepository.AddResolivingDomain(string resolvingKey, IResolvingDomain rdomain)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}