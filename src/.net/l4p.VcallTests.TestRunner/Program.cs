using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using l4p.VcallTests.Discovery;

namespace l4p.VcallTests.TestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var tests = new DiscoveryTests();
            tests.ManyToMany_load_test();
        }
    }
}
