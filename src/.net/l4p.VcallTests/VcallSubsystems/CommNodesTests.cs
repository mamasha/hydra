/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.VcallSubsystems;
using NUnit.Framework;

namespace l4p.VcallTests.VcallSubsystems
{
    [TestFixture]
    class CommNodesTests
    {
        [Test]
        public void ExplicitCloseProxy_should_pass()
        {
            var vcall = VcallSubsystem.New();

            var node = vcall.NewHosting();
            node.Close();

            var counters = vcall.Counters;

            Assert.That(counters.Vcall_Event_CloseCommPeer, Is.EqualTo(1));
            Assert.That(counters.HostingPeer_Event_IsStarted, Is.EqualTo(1));
            Assert.That(counters.HostingPeer_Event_IsStopped, Is.EqualTo(1));

            vcall.Stop();
        }

        [Test]
        public void ImplicitCloseProxy_should_pass()
        {
            var vcall = VcallSubsystem.New();

            var node = vcall.NewProxy();
            node.Close();

            var counters = vcall.Counters;

            Assert.That(counters.Vcall_Event_CloseCommPeer, Is.EqualTo(1));
            Assert.That(counters.ProxyPeer_Event_IsStarted, Is.EqualTo(1));
            Assert.That(counters.ProxyPeer_Event_IsStopped, Is.EqualTo(1));

            vcall.Stop();
        }

        [Test]
        public void ExplicitCloseHosting_should_pass()
        {
            var vcall = VcallSubsystem.New();
            vcall.NewHosting();
            vcall.Stop();

            var counters = vcall.Counters;

            Assert.That(counters.Vcall_Event_CloseCommPeer, Is.EqualTo(1));
            Assert.That(counters.HostingPeer_Event_IsStarted, Is.EqualTo(1));
            Assert.That(counters.HostingPeer_Event_IsStopped, Is.EqualTo(1));

            Console.WriteLine(counters);
        }

        [Test]
        public void ImplicitCloseHosting_should_pass()
        {
            var vcall = VcallSubsystem.New();
            vcall.NewProxy();
            vcall.Stop();

            var counters = vcall.Counters;

            Assert.That(counters.Vcall_Event_CloseCommPeer, Is.EqualTo(1));
            Assert.That(counters.ProxyPeer_Event_IsStarted, Is.EqualTo(1));
            Assert.That(counters.ProxyPeer_Event_IsStopped, Is.EqualTo(1));
        }
    }
}