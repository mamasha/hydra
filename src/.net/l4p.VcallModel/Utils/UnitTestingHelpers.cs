using System;
using System.Diagnostics;
using System.Threading;
using l4p.VcallModel.Core;

namespace l4p.VcallModel.Utils
{
    static class UnitTestingHelpers
    {
        public static void RunUpdateLoop(int timeout, Func<DebugCounters> getCounters)
        {
            var timer = Stopwatch.StartNew();

            for (;;)
            {
                Thread.Sleep(3000);

                try
                {
                    Console.WriteLine(getCounters());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                if (timer.ElapsedMilliseconds >= timeout)
                    break;
            }
        }
    }
}