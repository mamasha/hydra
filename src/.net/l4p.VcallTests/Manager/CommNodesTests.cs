/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Manager;
using NUnit.Framework;

namespace l4p.VcallTests.Manager
{
    [TestFixture]
    class CommNodesTests
    {
        [Test]
        public void ExplicitCloseTargets_should_pass()
        {
            var vcall = VcallSubsystem.New();

            var node = vcall.NewHosting();
            node.Close();

            var counters = vcall.Counters;

            Assert.That(counters.Vcall_Event_CloseCommNode, Is.EqualTo(1));
            Assert.That(counters.Hosting_Event_IsStarted, Is.EqualTo(1));
            Assert.That(counters.Hosting_Event_IsStopped, Is.EqualTo(1));

            vcall.Stop();
        }

        [Test]
        public void ImplicitCloseTargets_should_pass()
        {
            var vcall = VcallSubsystem.New();

            var node = vcall.NewTargets();
            node.Close();

            var counters = vcall.Counters;

            Assert.That(counters.Vcall_Event_CloseCommNode, Is.EqualTo(1));
            Assert.That(counters.Targets_Event_IsStarted, Is.EqualTo(1));
            Assert.That(counters.Targets_Event_IsStopped, Is.EqualTo(1));

            vcall.Stop();
        }

        [Test]
        public void ExplicitCloseHosting_should_pass()
        {
            var vcall = VcallSubsystem.New();
            vcall.NewHosting();
            vcall.Stop();

            var counters = vcall.Counters;

            Assert.That(counters.Vcall_Event_CloseCommNode, Is.EqualTo(1));
            Assert.That(counters.Hosting_Event_IsStarted, Is.EqualTo(1));
            Assert.That(counters.Hosting_Event_IsStopped, Is.EqualTo(1));

            Console.WriteLine(counters);
        }

        [Test]
        public void ImplicitCloseHosting_should_pass()
        {
            var vcall = VcallSubsystem.New();
            vcall.NewTargets();
            vcall.Stop();

            var counters = vcall.Counters;

            Assert.That(counters.Vcall_Event_CloseCommNode, Is.EqualTo(1));
            Assert.That(counters.Targets_Event_IsStarted, Is.EqualTo(1));
            Assert.That(counters.Targets_Event_IsStopped, Is.EqualTo(1));
        }
    }
}