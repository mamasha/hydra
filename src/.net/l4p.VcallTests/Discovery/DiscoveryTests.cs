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
using l4p.VcallModel.Configuration;
using l4p.VcallModel.Core;
using l4p.VcallModel.Utils;
using l4p.VcallModel.VcallSubsystems;
using NUnit.Framework;

namespace l4p.VcallTests.Discovery
{
    [TestFixture]
    public class DiscoveryTests
    {
        [Test, Explicit]
        public void SingleProxy_shoul_connect_to_hosting()
        {
            var vcall = VcallSubsystem.New("l4p.vcalltests");
            vcall.Start();

            var vproxy = vcall.NewProxy();

            UnitTestingHelpers.RunUpdateLoop(30*1000, () => vcall.Counters);

            vproxy.Close();
            vcall.Stop();

            Console.WriteLine(vcall.Counters.Format("Vcall is stopped"));
        }

        [Test, Explicit]
        public void SingleHosting_shoul_connect_to_proxy()
        {
            var vcall = VcallSubsystem.New("l4p.vcalltests");
            vcall.Start();

            vcall.NewHosting();

            UnitTestingHelpers.RunUpdateLoop(30 * 1000, () => vcall.Counters);

            vcall.Stop();

            Console.WriteLine(vcall.Counters.Format("Vcall is stopped"));
        }

        [Test, Explicit]
        public void SingleProxy_shoul_connect_to_hosting_then_gone()
        {
            var vproxy = Vcall.NewProxy();
            Thread.Sleep(10*1000);
            vproxy.Close();
        }

        [Test, Explicit]
        public void SingleHosting_shoul_connect_to_proxy_then_gone()
        {
            var hosting = Vcall.NewHosting();
            Thread.Sleep(10*1000);
            hosting.Close();
        }

        [Test, Explicit]
        public void SmellTest_should_get_self_notifications()
        {
            Console.WriteLine(Vcall.DebugCounters.Format("Vcall is initialized"));

            var hosting = Vcall.NewHosting();
            var vproxy = Vcall.NewProxy();

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

            vproxy.Close();
            Thread.Sleep(2000);

            Console.WriteLine(Vcall.DebugCounters.Format("All nodes are closed"));
        }

        [Test, Explicit]
        public void ManyToMany_should_get_self_notifications()
        {
            var vcall = VcallSubsystem.New();
            vcall.Start();

            const int count = 7;
            var random = new Random();

            var hosts = new IHosting[count];
            var vproxy = new IProxy[count];

            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(random.Next(100));

                hosts[i] = vcall.NewHosting();
                vproxy[i] = vcall.NewProxy();
            }

            UnitTestingHelpers.RunUpdateLoop(120 * 1000, () => vcall.Counters);

            vcall.Stop();

            Console.WriteLine(vcall.Counters.Format("Vcall is stopped"));
        }

        [Test, Explicit]
        public void ManyToMany_stress_test()
        {
            var vconfig = new VcallConfiguration
            {
                Logging = new LoggingConfiguration { ToFile = @"logs\vcall.log" }
            };

            var vcall = VcallSubsystem.New(vconfig);
            vcall.Start();

            const int count = 10;
            var random = new Random();

            var nodes = new ICommNode[count];

            int proxyCount = 0;
            int hostingCount = 0;

            Console.WriteLine("Spawning {0} nodes", count);

            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(random.Next(100));

                if (random.Next(100) % 2 == 0)
                {
                    nodes[i] = vcall.NewProxy();
                    proxyCount++;
                }
                else
                {
                    nodes[i] = vcall.NewHosting();
                    hostingCount++;
                }
            }

            var stopTimer = Stopwatch.StartNew();
            var printTimer = Stopwatch.StartNew();

            Console.WriteLine("Spawned {0} proxies and {1} hostings", proxyCount, hostingCount);

            for (;;)
            {
                Thread.Sleep(random.Next(10));

                int indx = random.Next(count);

                nodes[indx].Close();

                if (random.Next(100) % 2 == 0)
                {
                    nodes[indx] = vcall.NewProxy();
                    proxyCount++;
                }
                else
                {
                    nodes[indx] = vcall.NewHosting();
                    hostingCount++;
                }

                if (printTimer.ElapsedMilliseconds > 5000)
                {
                    Console.WriteLine(vcall.Counters.Format("Spawned {0} proxies and {1} hostings", proxyCount, hostingCount));
                    printTimer.Restart();
                }

                if (stopTimer.ElapsedMilliseconds > 120 * 1000)
                    break;
            }

            Console.WriteLine("Spawned {0} proxies and {1} hostings", proxyCount, hostingCount);

            for (int i = 0; i < count; i++)
            {
                nodes[i].Close();
            }

            UnitTestingHelpers.RunUpdateLoop(30 * 1000, () => vcall.Counters);

            vcall.Stop();

            UnitTestingHelpers.RunUpdateLoop(20 * 1000, () => vcall.Counters);
        }

        [Test, Explicit]
        public void ManyToMany_load_test()
        {
            var vconfig = new VcallConfiguration
            {
                Logging = new LoggingConfiguration { ToFile = @"logs\vcall.log" }
            };

            var vcall = VcallSubsystem.New(vconfig);
            vcall.Start();

            const int count = 30;
            var random = new Random();

            var nodes = new ICommNode[count];
            var timers = new Stopwatch[count];

            int proxyCount = 0;
            int hostingCount = 0;
            int closedCount = 0;

            Action<int> NewNode = indx =>
            {
                if (nodes[indx] != null)
                {
                    closedCount++;
                    nodes[indx].Close();
                }

                if (random.Next(100) % 2 == 0)
                {
                    nodes[indx] = vcall.NewProxy();
                    proxyCount++;
                }
                else
                {
                    nodes[indx] = vcall.NewHosting();
                    hostingCount++;
                }

                timers[indx] = Stopwatch.StartNew();
            };

            Console.WriteLine("Spawning {0} nodes", count);

            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(random.Next(100));
                NewNode(i);
            }

            var stopTimer = Stopwatch.StartNew();
            var printTimer = Stopwatch.StartNew();

            Console.WriteLine("Spawned {0} proxies and {1} hostings", proxyCount, hostingCount);

            for (;;)
            {
                Thread.Sleep(random.Next(10));

                int indx = random.Next(count);

                if (timers[indx].ElapsedMilliseconds > 3 * 1000)
                    NewNode(indx);

                if (printTimer.ElapsedMilliseconds > 5000)
                {
                    Console.WriteLine(vcall.Counters.Format("Spawned {0} proxies and {1} hostings (closed={2})", proxyCount, hostingCount, closedCount));
                    printTimer.Restart();
                }

                if (stopTimer.ElapsedMilliseconds > 10 * 60 * 1000)
                    break;
            }

            Console.WriteLine("Spawned {0} proxies and {1} hostings (closed={2})", proxyCount, hostingCount, closedCount);

            for (int i = 0; i < count; i++)
            {
                nodes[i].Close();
            }

            UnitTestingHelpers.RunUpdateLoop(30 * 1000, () => vcall.Counters);

            vcall.Stop();

            UnitTestingHelpers.RunUpdateLoop(20 * 1000, () => vcall.Counters);
        }
    }
}