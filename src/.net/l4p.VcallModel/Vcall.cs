/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Core;
using l4p.VcallModel.Helpers;

namespace l4p.VcallModel
{
    public class VcallException : Exception
    {
        public VcallException() { }
        public VcallException(string message) : base(message) { }
        public VcallException(string message, Exception inner) : base(message, inner) { }
    }

    public static class Vcall
    {
        #region members

        private static readonly ILogger _log = Logger.New("Vcall");
        private static readonly IHelpers Helpers = Utils.New(_log);

        private static readonly object _startMutex = new Object();
        private static IVimpl _vimpl;

        #endregion

        #region construction

        static Vcall()
        { }

        #endregion

        #region private

        private static void assert_services_are_started()
        {
            if (_vimpl != null)
                return;

            throw
                Helpers.MakeNew<VcallException>(null, "Vcall has not been initialized. Call StartServices() first.");
        }

        private static void start_services()
        {
            if (_vimpl != null)
                return;

            _vimpl = Vimpl.New();

            _log.Info("Vcall services are started");
        }

        private static void stop_services()
        {
            if (_vimpl == null)
                return;

            _vimpl = null;

            _log.Info("Vcall services are stopped");
        }

        #endregion

        #region API

        /// <summary>
        /// Initialize and start essential services of Vcall system.
        /// Should be called before any other Vcall functionality is accessed </summary>
        /// <remarks>Subsiquent calls to StartServices() are ignored</remarks>
        public static void StartServices()
        {
            lock (_startMutex)
            {
                start_services();
            }
        }

        /// <summary>
        /// Gracefully stop and release any services of Vcall system.
        /// No Vcall functionality maybe accessed after this call. </summary>
        /// <remarks>To re-initialize Vcall system call StartServices() again
        /// If services are not started StopServices() call is ignored.
        public static void StopServices()
        {
            lock (_startMutex)
            {
                stop_services();
            }
        }

        /// <summary>
        /// Get default target model </summary>
        public static IVtarget DefaultTarget
        {
            get
            {
                assert_services_are_started();
                return _vimpl;
            }
        }

        /// <summary>
        /// Get default hosting ... </summary>
        public static IVhosting DefaultHosting
        {
            get
            {
                assert_services_are_started();
                return _vimpl;
            }
        }

        /// <summary>
        /// Create new hosting model with default parameters ... </summary>
        /// <returns></returns>
        public static IVhosting NewHosting()
        {
            assert_services_are_started();
            return _vimpl;
        }

        /// <summary>
        /// Create new target model with default parameters ...</summary>
        /// <returns></returns>
        public static IVtarget NewTarget()
        {
            assert_services_are_started();
            return _vimpl;
        }

        /// <summary>
        /// Create custom hosting model</summary>
        /// <param name="config">Custom parameters</param>
        /// <returns>New hosting with custom parameters</returns>
        public static IVhosting NewHosting(HostingConfiguration config)
        {
            assert_services_are_started();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create new custom target model</summary>
        /// <param name="config">Costomization parameters</param>
        /// <returns>New target model</returns>
        public static IVtarget NewTarget(TargetConfiguration config)
        {
            assert_services_are_started();
            throw new NotImplementedException();
        }

        #endregion
    }
}