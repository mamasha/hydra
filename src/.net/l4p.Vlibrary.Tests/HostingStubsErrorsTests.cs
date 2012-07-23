using l4p.VcallModel;
using NUnit.Framework;

namespace l4p.Vlibrary.Tests
{
    [TestFixture]
    class HostingStubsErrorsTests
    {
        [SetUp] void StartVcallSerives() { Vcall.StartServices(); }
        [TearDown] void StopVcallServices() { Vcall.StopServices(); }

        [Test]
        public void CallFoo_should_invoke_remote_Foo()
        {
            var vtarget = Vcall.GetTargetsAt("StubsHosting");
            vtarget.Call("SomeMissingFunctionName");
        }

        [Test]
        public void CallUnregisteredFunction_should_throw()
        {
            var vtarget = Vcall.GetTargetsAt("StubsHosting");
            vtarget.Call("SomeMissingFunctionName");
        }
    }
}