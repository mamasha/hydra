/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;

namespace l4p.VcallModel
{
    public static class Vcore
    {
        #region members

        private static readonly IVimpl _vimpl;

        #endregion

        #region construction

        static Vcore()
        {
            _vimpl = Vimpl.New();
        }

        #endregion

        #region API

        /// <summary>
        /// Get default target model </summary>
        public static IVcall DefaultTarget
        {
            get { return _vimpl; }
        }

        /// <summary>
        /// Get default hosting ... </summary>
        public static IVhost DefaultHosting
        {
            get { return _vimpl; }
        }

        /// <summary>
        /// Create new hosting model with default parameters ... </summary>
        /// <returns></returns>
        public static IVhost NewHosting()
        {
            return _vimpl;
        }

        /// <summary>
        /// Create new target model with default parameters ...</summary>
        /// <returns></returns>
        public static IVcall NewTarget()
        {
            return _vimpl;
        }

        /// <summary>
        /// Create custom hosting model</summary>
        /// <param name="config">Custom parameters</param>
        /// <returns>New hosting with custom parameters</returns>
        public static IVhost NewHosting(HostingConfiguration config)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create new custom target model</summary>
        /// <param name="config">Costomization parameters</param>
        /// <returns>New target model</returns>
        public static IVcall NewTarget(TargetConfiguration config)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}