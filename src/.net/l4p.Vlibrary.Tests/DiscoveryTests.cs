using System;
using System.Threading;
using l4p.VcallModel;
using NUnit.Framework;

namespace l4p.Vlibrary.Tests
{
    [TestFixture]
    class DiscoveryTests
    {
        [SetUp] void StartVcallSerives() { Vcall.StartServices(); }
        [TearDown] void StopVcallServices() { Vcall.StopServices(); }

        [Test]
        public void SmellTest_should_get_self_notifications()
        {
            var hosting = Vcall.NewHosting("PublishSubscribe_should_get_self_notifications");
            var targets = Vcall.GetTargetsAt("PublishSubscribe_should_get_self_notifications");

            Thread.Sleep(50000);
        }


        [Test]
        public void ManyToMany_should_get_self_notifications()
        {
            const int count = 3;
            var random = new Random();

            var hosts = new IVhosting[count];
            var targets = new IVtarget[count];

            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(random.Next(100));

                hosts[i] = Vcall.NewHosting("LoadTest");
                targets[i] = Vcall.GetTargetsAt("LoadTest");
            }

            Thread.Sleep(1000000);
        }
    }
}