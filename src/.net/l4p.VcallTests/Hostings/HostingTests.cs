/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel;
using l4p.VcallModel.Core;
using l4p.VcallModel.HostingPeers;
using l4p.VcallModel.Hostings;
using l4p.VcallModel.Utils;
using l4p.VcallModel.VcallSubsystems;
using Moq;
using NUnit.Framework;

namespace l4p.VcallTests.Hostings
{
    [TestFixture]
    class HostingTests
    {
        private ICountersDb _countersDb;

        [SetUp]
        public void SetContext()
        {
            Context.Clear();

            _countersDb = CountersDb.New();
            Context.With(_countersDb);
        }

        [Test]
        public void HandleNewProxy()
        {
            var vcall = VcallSubsystem.New();
            var config = new HostingConfiguration();

            var peer = new HostingPeer(config, vcall);

            var info = new ProxyInfo
            {
                Tag = Guid.NewGuid().ToString(),
                ListeningUri = "/tests",
                NameSpace = "Vcall.Testing",
                HostName = "localhost"
            };

            var hosting = new Hosting(peer, vcall, config);
            hosting.HandleNewProxy(info);
        }

        [Test]
        public void HandleNewProxy_NotMyNamespace_should_filter_proxy()
        {
            var vcall = new Mock<IVcallSubsystem>();
            var peer = new Mock<ICommPeer>();

            var hosting = new Hosting(peer.Object, vcall.Object, new HostingConfiguration {NameSpace = "Vcall.Testing"});
            hosting.HandleNewProxy(new ProxyInfo {NameSpace = "Other.Testing"});

            var counters = _countersDb.SumAll();
            Assert.That(counters.Hosting_Event_NotMyNamespace, Is.EqualTo(1));
        }
    }
}