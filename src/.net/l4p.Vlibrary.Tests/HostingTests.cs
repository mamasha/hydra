using System;
using l4p.VcallModel;
using NUnit.Framework;

namespace l4p.Vlibrary.Tests
{
    [TestFixture]
    class HostingTests
    {
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
        public void HostSingleFunction_should_host_a_function()
        {
            var vhost = Vcore.NewHosting();
            var vcall = Vcore.NewTarget();

            vhost.AddTarget(Foo);
            vcall.Call(() => Foo());

            Assert.That(_fooIsCalled, Is.True);
        }
    }
}