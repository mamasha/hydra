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

    public sealed class Vcall
    {
        #region members

        private static readonly ILogger _log = Logger.New<Vcall>();
        private static readonly IHelpers Helpers = Utils.New(_log);

        private static readonly object _startMutex = new Object();

        private static VcallConfiguration _config;
        private static IVcallSubsystem _vcall;
        private static IVhosting _defaultHosting;
        private static IVtarget _defaultTarget;

        #endregion

        #region construction

        static Vcall()
        { }

        private Vcall()
        { }

        #endregion

        #region private

        private static void assert_services_are_started()
        {
            if (_vcall != null)
                return;

            throw
                Helpers.MakeNew<VcallException>(null, "Vcall has not been initialized. Call StartServices() first.");
        }

        private static void start_services(VcallConfiguration config)
        {
            if (_vcall != null)
                return;

            var vcall = Helpers.TryCatch(
                () => VcallSubsystem.New(config),
                ex => Helpers.ThrowNew<VcallException>(ex, "Failed to create Vcall subsystem"));

            Helpers.TryCatch(
                () => vcall.Start(),
                ex => Helpers.ThrowNew<VcallException>(ex, "Failed to start Vcall subsystem"));

            _config = config;
            _vcall = vcall;
            _defaultHosting = null;
            _defaultTarget = null;

            _log.Info("Vcall services are started");
        }

        private static void stop_services()
        {
            if (_vcall == null)
                return;

            var defaultHosting = _defaultHosting;
            var defaultTarget = _defaultTarget;
            var vcall = _vcall;

            _defaultHosting = null;
            _defaultTarget = null;
            _config = null;
            _vcall = null;

            try
            {
                if (defaultHosting != null)
                    defaultHosting.Close();

                if (defaultTarget != null)
                    defaultTarget.Close();

                Helpers.TryCatch(
                    () => vcall.Stop(),
                    ex => Helpers.ThrowNew<VcallException>(ex, "Failed to stop default Vcall endpoint services"));
            }
            catch (Exception ex)
            {
                _log.Error(ex.GetDetailedMessage());
            }

            _log.Info("Vcall services are stopped");
        }

        private static IVhosting new_hosting(HostingConfiguration config)
        {
            try
            {
                return
                    _vcall.NewHosting(config);
            }
            catch (Exception ex)
            {
                throw
                    Helpers.MakeNew<VcallException>(ex, "[{0}] failed to create hosting", config.ResolvingKey);
            }
        }

        private static IVtarget new_target(TargetConfiguration config)
        {
            try
            {
                return
                    _vcall.NewTarget(config);
            }
            catch (Exception ex)
            {
                throw
                    Helpers.MakeNew<VcallException>(ex, "[{0}] failed to create targets", config.ResolvingKey);
            }
        }

        private static IVhosting get_default_hosting()
        {
            if (_defaultHosting != null)
                return _defaultHosting;

            _defaultHosting = new_hosting(new HostingConfiguration
                                              {
                                                  ResolvingKey = _config.ResolvingKey
                                              });
            return _defaultHosting;
        }

        private static IVtarget get_default_target()
        {
            if (_defaultTarget == null)
                return _defaultTarget;

            _defaultTarget = new_target(new TargetConfiguration
                                            {
                                                ResolvingKey = _config.ResolvingKey
                                            });

            return _defaultTarget;
        }
        #endregion

        #region API

        /// <summary>
        /// Initialize and start essential services of Vcall system.
        /// Should be called before any other Vcall functionality is accessed </summary>
        /// <remarks>Subsiquent calls to StartServices() are ignored</remarks>
        public static void StartServices()
        {
            var config = new VcallConfiguration();

            lock (_startMutex)
            {
                start_services(config);
            }
        }

        /// <summary>
        /// Initialize and start essential services of Vcall system.
        /// Should be called before any other Vcall functionality is accessed </summary>
        /// <param name="config">Configuration to be used</param>
        /// <remarks>Subsiquent calls to StartServices() are ignored</remarks>
        public static void StartServices(VcallConfiguration config)
        {
            lock (_startMutex)
            {
                start_services(config);
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
        public static IVtarget DefaultTargets
        {
            get
            {
                assert_services_are_started();

                lock (_startMutex)
                {
                    return
                        get_default_target();
                }
            }
        }

        /// <summary>
        /// Get default hosting ... </summary>
        public static IVhosting DefaultHosting
        {
            get
            {
                assert_services_are_started();

                lock (_startMutex)
                {
                    return
                        get_default_hosting();
                }
            }
        }

        /// <summary>
        /// Create new hosting model with default parameters ... </summary>
        /// <returns></returns>
        public static IVhosting NewHosting(string resolvingKey)
        {
            assert_services_are_started();
            return
                new_hosting(new HostingConfiguration
                                {
                                    ResolvingKey = resolvingKey
                                });
        }

        /// <summary>
        /// Create custom hosting model</summary>
        /// <param name="config">Custom parameters</param>
        /// <returns>New hosting with custom parameters</returns>
        public static IVhosting NewHosting(HostingConfiguration config)
        {
            assert_services_are_started();
            return
                new_hosting(config);
        }

        /// <summary>
        /// Create new target model with default parameters ...</summary>
        /// <returns></returns>
        public static IVtarget GetTargetsAt(string resolvingKey)
        {
            assert_services_are_started();
            return
                _vcall.NewTarget(new TargetConfiguration
                                     {
                                         ResolvingKey = resolvingKey
                                     });
        }

        /// <summary>
        /// Create new custom target model</summary>
        /// <param name="config">Costomization parameters</param>
        /// <returns>New target model</returns>
        public static IVtarget GetTargetsAt(TargetConfiguration config)
        {
                assert_services_are_started();
                return
                    new_target(config);
        }

        #endregion
    }
}