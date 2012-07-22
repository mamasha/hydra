using l4p.VcallModel;
using NUnit.Framework;

namespace l4p.Vlibrary.Tests
{
    [TestFixture]
    class DefaultHostingStubsTests
    {
        [SetUp] void StartVcallSerives() { Vcall.StartServices(); }
        [TearDown] void StopVcallServices() { Vcall.StopServices(); }

        [Test, ExpectedException(typeof(VcallException), ExpectedMessage = "no registered targets", MatchType = MessageMatch.Contains)]
        public void CallNotExisingFunction_should_throw()
        {
            Vcall.DefaultTarget.Call("SomeFunction");
        }

        [Test]
        public void CallFoo_should_invoke_remote_Foo()
        {
            Vcall.DefaultTarget.Call("foo");
        }
    }
}