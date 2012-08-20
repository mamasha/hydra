using System;
using l4p.VcallModel;
using l4p.VcallModel.Configuration;
using l4p.VcallModel.Core;
using l4p.VcallModel.Manager;

namespace l4p.VcallTests.StubsHosting
{
    class DefaultHosting
    {
        private static void Foo()
        {
            Console.WriteLine("Foo() is called");
        }
    }
}