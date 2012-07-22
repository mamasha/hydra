/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

namespace l4p.VcallModel.Helpers
{
    interface IHelpers
    {
        ILogger _log { get; }
    }

    class Utils : IHelpers
    {
        #region members

        private readonly ILogger _log;

        #endregion

        #region construction

        public static IHelpers New(ILogger log)
        {
            return 
                new Utils(log);
        }

        private Utils(ILogger log)
        {
            _log = log;
        }

        #endregion

        #region IHelpers

        ILogger IHelpers._log
        {
            get { return _log; }
        }

        #endregion
    }
}