using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using l4p.VcallModel;

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

        static void parse_appconfig(HostingConfiguration config)
        {
            config.ResolvingKey = ConfigurationManager.AppSettings["ResolvingKey"];

            string port = ConfigurationManager.AppSettings["port"];
            if (port != null)
            {
                config.Port = Int32.Parse(port);
            }
        }

        static void parse_command_line(string[] args, HostingConfiguration config)
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
                        config.Port = Int32.Parse(prm);
                    }
                    catch (FormatException)
                    {
                        throw NewArgumentException("-port: failed to parse '%s'", prm);
                    }

                    indx += 2;
                }
                else
                if (token == "-")
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
                config.ResolvingKey = resolvingKey;
            }
        }

        static void main_impl(string[] args)
        {
            var config = new HostingConfiguration();

            try
            {
                parse_appconfig(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed to parse application configuration: {0}", ex.Message);
            }

            parse_command_line(args, config);

            DefaultHosting.HostStubs();

            Console.WriteLine();
            Console.WriteLine("Hosting is running (discovery is turned on)");
            Console.WriteLine("press Ctrl-C to stop");
            Console.WriteLine();

            for (;;)
            {
                try
                {
                    Thread.Sleep(1000);
                }
                catch
                {
                    break;
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("l4p.VcallTests.StubsHosting (-help for usage)");
            Console.WriteLine("");

            try
            {
                Vcall.StartServices();

                main_impl(args);

                Vcall.StopServices();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
