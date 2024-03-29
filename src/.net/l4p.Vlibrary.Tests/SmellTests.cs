﻿using System;
using l4p.VcallModel;
using NUnit.Framework;

namespace l4p.Vlibrary.Tests
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
            string key = Guid.NewGuid().ToString("B");
            var vhost = Vcall.NewHosting(key);
            var vtarget = Vcall.GetTargetsAt(key);

            vhost.AddTarget(Foo);
            vtarget.Call(() => Foo());

            Assert.That(_fooIsCalled, Is.True);
        }
    }
}