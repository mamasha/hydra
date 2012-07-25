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
    class HostingStubsErrorsTests
    {
        [SetUp] void StartVcallSerives() { Vcall.StartServices(); }
        [TearDown] void StopVcallServices() { Vcall.StopServices(); }

        [Test]
        public void CallFoo_should_invoke_remote_Foo()
        {
            var vtarget = Vcall.GetTargets();
            vtarget.Call("SomeMissingFunctionName");
        }

        [Test]
        public void CallUnregisteredFunction_should_throw()
        {
            var vtarget = Vcall.GetTargets();
            vtarget.Call("SomeMissingFunctionName");
        }
    }
}