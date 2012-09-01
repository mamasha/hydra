/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel;
using NUnit.Framework;

namespace l4p.VcallTests
{
    [TestFixture]
    class SmellTests
    {
        [SetUp] void StartVcallSerives() { Vcall.StartServices(); }
        [TearDown] void StopVcallServices() { Vcall.StopServices(); }

        private bool _fooIsCalled;

        [SetUp]
        public void ClearState()
        {
            _fooIsCalled = false;
        }

        private void Foo()
        {
            _fooIsCalled = true;
        }

        [Test]
        public void LocalHostSingleFunction_should_call_a_function()
        {
            var vhost = Vcall.NewHosting();
            var vproxy = Vcall.NewProxy();

            vhost.Host(Foo);
            vproxy.Call(() => Foo());

            Assert.That(_fooIsCalled, Is.True);
        }
    }
}