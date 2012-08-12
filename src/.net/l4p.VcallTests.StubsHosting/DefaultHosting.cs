using System;
using l4p.VcallModel;

namespace l4p.VcallTests.StubsHosting
{
    class DefaultHosting
    {
        private static void Foo()
        {
            Console.WriteLine("Foo() is called");
        }

        public static void HostStubs()
        {
            var host = Vcall.NewHosting();
            host.AddTarget(Foo);
        }

        public static void StartSingleEmptyHosting()
        {
            Vcall.NewHosting();
        }
    }
}