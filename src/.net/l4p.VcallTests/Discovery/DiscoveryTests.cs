/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Diagnostics;
using System.Threading;
using l4p.VcallModel;
using l4p.VcallModel.Core;
using NUnit.Framework;

namespace l4p.VcallTests.Discovery
{
    [TestFixture]
    class DiscoveryTests
    {
        [SetUp] void StartVcallSerives() { Vcall.StartServices("l4p.vcalltests"); }
        [TearDown] void StopVcallServices() { Vcall.StopServices(); }

        [Test, Explicit]
        public void SingleTargets_shoul_connect_to_hosting()
        {
            var targets = Vcall.GetTargets();

            Thread.Sleep(5000*1000);

            targets.Close();
        }

        [Test, Explicit]
        public void SingleHosting_shoul_connect_to_targets()
        {
            var hosting = Vcall.NewHosting();

            Thread.Sleep(1000 * 1000);

            hosting.Close();
        }

        [Test, Explicit]
        public void SmellTest_should_get_self_notifications()
        {
            Console.WriteLine(Vcall.DebugCounters.Format("Vcall is initialized"));

            var hosting = Vcall.NewHosting();
            var targets = Vcall.GetTargets();

            Thread.Sleep(5000);
            Console.WriteLine(Vcall.DebugCounters.Format("All nodes are active"));

            for (var timer = Stopwatch.StartNew(); timer.ElapsedMilliseconds < 30000; )
            {
                Thread.Sleep(3000);
                Console.WriteLine(Vcall.DebugCounters.Format("Active state"));
            }

            hosting.Close();

            for (var timer = Stopwatch.StartNew(); timer.ElapsedMilliseconds < 30000; )
            {
                Thread.Sleep(3000);
                Console.WriteLine(Vcall.DebugCounters.Format("No active hosts"));
            }

            targets.Close();
            Thread.Sleep(2000);

            Console.WriteLine(Vcall.DebugCounters.Format("All nodes are closed"));
        }

        [Test, Explicit]
        public void ManyToMany_should_get_self_notifications()
        {
            const int count = 3;
            var random = new Random();

            var hosts = new IVhosting[count];
            var targets = new IVtarget[count];

            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(random.Next(100));

                hosts[i] = Vcall.NewHosting();
                targets[i] = Vcall.GetTargets();
            }

            for (var timer = Stopwatch.StartNew(); timer.ElapsedMilliseconds < 120*1000; )
            {
                Thread.Sleep(3000);
                Console.WriteLine(Vcall.DebugCounters.Format("No active hosts"));
            }
        }
    }
}