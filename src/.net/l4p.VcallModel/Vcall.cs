/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Configuration;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel
{
    public class VcallException : VcallModelException
    {
        public VcallException() { }
        public VcallException(string message) : base(message) { }
        public VcallException(string message, Exception inner) : base(message, inner) { }
    }

    public sealed class Vcall
    {
        #region members

        private static readonly ILogger _log = Logger.New<Vcall>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private static readonly object _startMutex = new Object();

        private static IVcallSubsystem _core;
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
            if (_core != null)
                return;

            throw
                Helpers.MakeNew<VcallException>(null, _log, "Vcall has not been initialized. Call StartServices() first.");
        }

        private static void start_services(VcallConfiguration vconfig)
        {
            if (_core != null)
                return;

            var core = Helpers.TryCatch(_log,
                () => VcallSubsystem.New(vconfig),
                ex => Helpers.ThrowNew<VcallException>(ex, _log, "Failed to create Vcall subsystem"));

            Helpers.TryCatch(_log,
                () => core.Start(),
                ex => Helpers.ThrowNew<VcallException>(ex, _log, "Failed to start Vcall subsystem"));

            _core = core;
            _defaultHosting = null;
            _defaultTarget = null;

            _log.Info("Vcall services are started");
        }

        private static void stop_services()
        {
            if (_core == null)
                return;

            var defaultHosting = _defaultHosting;
            var defaultTarget = _defaultTarget;
            var core = _core;

            _defaultHosting = null;
            _defaultTarget = null;
            _core = null;

            try
            {
                if (defaultHosting != null)
                    defaultHosting.Close();

                if (defaultTarget != null)
                    defaultTarget.Close();

                Helpers.TryCatch(_log,
                    () => core.Stop(),
                    ex => Helpers.ThrowNew<VcallException>(ex, _log, "Failed to stop default Vcall endpoint services"));
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
                    _core.NewHosting(config);
            }
            catch (Exception ex)
            {
                throw
                    Helpers.MakeNew<VcallException>(ex, _log, "Failed to create hosting");
            }
        }

        private static IVtarget new_target(TargetConfiguration config)
        {
            try
            {
                return
                    _core.NewTargets(config);
            }
            catch (Exception ex)
            {
                throw
                    Helpers.MakeNew<VcallException>(ex, _log, "Failed to create target");
            }
        }

        private static IVhosting get_default_hosting()
        {
            if (_defaultHosting != null)
                return _defaultHosting;

            _defaultHosting = new_hosting(new HostingConfiguration());

            return _defaultHosting;
        }

        private static IVtarget get_default_target()
        {
            if (_defaultTarget == null)
                return _defaultTarget;

            _defaultTarget = new_target(new TargetConfiguration());

            return _defaultTarget;
        }
        #endregion

        #region API

        /// <summary>
        /// Gets the actual configuration of a running Vcall system
        /// </summary>
        public static VcallConfiguration Config
        {
            get
            {
                assert_services_are_started();
                return _core.Config;
            }
        }

        /// <summary>
        /// Initialize and start essential services of Vcall system.
        /// Should be called before any other Vcall functionality is accessed </summary>
        /// <remarks>Subsiquent calls to StartServices() are ignored</remarks>
        public static void StartServices()
        {
            var vconfig = new VcallConfiguration();

            lock (_startMutex)
            {
                start_services(vconfig);
            }
        }

        /// <summary>
        /// Initialize and start essential services of Vcall system.
        /// Should be called before any other Vcall functionality is accessed </summary>
        /// <param name="resolvingKey">Resolving key of ...</param>
        /// <remarks>Subsiquent calls to StartServices() are ignored</remarks>
        public static void StartServices(string resolvingKey)
        {
            var vconfig = new VcallConfiguration
            {
                ResolvingKey = resolvingKey
            };

            lock (_startMutex)
            {
                start_services(vconfig);
            }
        }

        /// <summary>
        /// Initialize and start essential services of Vcall system.
        /// Should be called before any other Vcall functionality is accessed </summary>
        /// <param name="vconfig">Configuration to be used</param>
        /// <remarks>Subsiquent calls to StartServices() are ignored</remarks>
        public static void StartServices(VcallConfiguration vconfig)
        {
            lock (_startMutex)
            {
                start_services(vconfig);
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
        /// <returns>New active hosting that is ready to host functions</returns>
        public static IVhosting NewHosting()
        {
            assert_services_are_started();
            return
                new_hosting(new HostingConfiguration());
        }

        /// <summary>
        /// Create new hosting model with default parameters ... </summary>
        /// <param name="visibilityScope">Hosting visibility scope (usually local host or resolving domain)</param>
        /// <returns>New active hosting that is ready to host functions</returns>
        public static IVhosting NewHosting(HostingVisibilityScope visibilityScope)
        {
            assert_services_are_started();
            var config = new HostingConfiguration { VisibilityScope = visibilityScope };
            return
                new_hosting(config);
        }

        /// <summary>
        /// Create new hosting model with default parameters ... </summary>
        /// <param name="namespace">A namespace for functions hosted by a returned hosting</param>
        /// <returns>New active hosting that is ready to host functions</returns>
        public static IVhosting NewHosting(string @namespace)
        {
            assert_services_are_started();
            var config = new HostingConfiguration { NameSpace = @namespace };
            return
                new_hosting(config);
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
        /// <returns>A new proxy on which v-calls are issued</returns>
        public static IVtarget GetTargets()
        {
            assert_services_are_started();
            return
                _core.NewTargets(new TargetConfiguration());
        }

        /// <summary>
        /// Create new target model with default parameters ...</summary>
        /// <param name="namespace">Restrict v-calls resolving to a specific namespace</param>
        /// <returns>A new proxy on which v-calls are issued</returns>
        public static IVtarget GetTargets(string @namespace)
        {
            assert_services_are_started();
            return
                _core.NewTargets(new TargetConfiguration { NameSpace = @namespace });
        }

        /// <summary>
        /// Create new custom target model</summary>
        /// <param name="config">Costomization parameters</param>
        /// <returns>A new proxy on which v-calls are issued</returns>
        public static IVtarget GetTargets(TargetConfiguration config)
        {
                assert_services_are_started();
                return
                    new_target(config);
        }

        /// <summary>
        /// Get internal debug counters </summary>
        public static DebugCounters DebugCounters
        {
            get { return _core.DebugCounters; }
        }

        #endregion
    }
}