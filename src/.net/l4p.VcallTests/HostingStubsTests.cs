/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using l4p.VcallModel;
using NUnit.Framework;

namespace l4p.VcallTests
{
    [TestFixture]
    class HostingStubsTests
    {
        [SetUp] void StartVcallSerives() { Vcall.StartServices(); }
        [TearDown] void StopVcallServices() { Vcall.StopServices(); }

        [Test, ExpectedException(typeof(VcallException), ExpectedMessage = "no registered targets", MatchType = MessageMatch.Contains)]
        public void CallNotExisingFunction_should_throw()
        {
            Vcall.DefaultTargets.Call("SomeFunction");
        }

        [Test]
        public void CallFoo_should_invoke_remote_Foo()
        {
            var vtarget = Vcall.GetTargets();
            vtarget.Call("Foo");
        }
    }
}