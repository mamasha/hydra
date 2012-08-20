using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using l4p.VcallModel;
using l4p.VcallModel.Configuration;
using l4p.VcallModel.Utils;

namespace l4p.VcallTests.StubsHosting
{
    class Program
    {
        private static ArgumentException NewArgumentException(string format, params object[] args)
        {
            string errMsg = format;

            try
            {
                errMsg = String.Format(format, args);
            }
            catch { }

            return
                new ArgumentException(errMsg);
        }

        static void parse_appconfig(VcallConfiguration vconfig)
        {
            string resolvingKey = ConfigurationManager.AppSettings["ResolvingKey"];
            string port = ConfigurationManager.AppSettings["port"];

            if (String.IsNullOrEmpty(resolvingKey))
            {
                resolvingKey = "l4p.vcalltests";
            }

            vconfig.ResolvingKey = resolvingKey;

            if (port != null)
            {
                vconfig.Port = Int32.Parse(port);
            }
        }

        static void parse_command_line(string[] args, VcallConfiguration vconfig)
        {
            int argc = args.Length;

            List<string> unnamed = new List<string>();

            int indx = 0;

            for (;;)
            {
                if (indx == argc)
                    break;

                String token = args[indx].ToLowerInvariant();
                String prm = null;

                if (indx + 1 < argc)
                    prm = args[indx + 1];

                if (token == "-port")
                {
                    if (prm == null)
                        throw NewArgumentException("-port: please specify a tcp port number");

                    try
                    {
                        vconfig.Port = Int32.Parse(prm);
                    }
                    catch (FormatException)
                    {
                        throw NewArgumentException("-port: failed to parse '%s'", prm);
                    }

                    indx += 2;
                }
                else
                if (token == "-abrakadabra")
                {
                }
                else
                {
                    unnamed.Add(token);
                    indx += 1;
                }
            }

            if (unnamed.Count == 0)
            {
                Console.WriteLine("No resolving key is specified; using defualt resolving domain");
            }

            if (unnamed.Count == 1)
            {
                string resolvingKey = unnamed[0];
                Console.WriteLine("Using resolving key '{0}'", resolvingKey);
                vconfig.ResolvingKey = resolvingKey;
            }
        }

        static void main_impl(string[] args)
        {
            var vconfig = new VcallConfiguration();

            try
            {
                parse_appconfig(vconfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed to parse application configuration: {0}", ex.Message);
            }

            parse_command_line(args, vconfig);

            Vcall.StartServices(vconfig);

            Vcall.NewHosting();
            Vcall.GetTargets();

            Console.WriteLine();
            Console.WriteLine("Hosting is running (discovery is on)");
            Console.WriteLine("press Ctrl-C to stop");
            Console.WriteLine();

            UnitTestingHelpers.RunUpdateLoop(10 * 60 * 1000, () => Vcall.DebugCounters);

            Vcall.StopServices();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("l4p.VcallTests.StubsHosting (-help for usage)");
            Console.WriteLine("");

            try
            {
                main_impl(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
