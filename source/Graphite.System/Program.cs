using System;
using System.Configuration;
using System.Linq;
using Graphite.Configuration;
using Graphite.System.Configuration;
using Topshelf;

namespace Graphite.System
{
    public static class Program
    {
        public static void Main(params string[] parameter)
        {
            if (Environment.UserInteractive)
            {
                // Start as console...
                Func<string, bool> isParamater = (s) => s != null && (s.StartsWith("-") || s.StartsWith("/"));
                Func<string, bool> isEParamater = (s) => s == "-e" || s == "/e";
                Func<string, bool> isHParamater = (s) => s == "-h" || s == "/h" || s == "-?" || s == "/?" || s == "--help" || s == "/help";
                Func<string, bool> isIParamater = (s) => s == "-i" || s == "/i";

                if (parameter != null && parameter.Any(isHParamater))
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine();
                    Console.WriteLine("[no parameters] -> Start listening on configured PerformanceCounters (App.config)");
                    Console.WriteLine("-e [category] [instance] -> Explore PerformanceCounters (all or by category or by category and instance)");
                    Console.WriteLine();
                }
                else if (parameter != null && parameter.Any(isEParamater))
                {
                    string[] path = parameter
                        .SkipWhile(s => !isEParamater(s))
                        .Skip(1)
                        .TakeWhile(s => !isParamater(s))
                        .ToArray();

                    Explorer.Print(path);
                }
                else if (parameter != null && parameter.Any(isIParamater))
                {
                    Inspector.Print(GraphiteSystemConfiguration.Instance.CounterListeners.OfType<CounterListenerElement>());
                }
                else
                {
                    using (new Kernel(GraphiteConfiguration.Instance, GraphiteSystemConfiguration.Instance))
                    {
                        Console.WriteLine("Listening on configured performance counters...");
                        Console.WriteLine("Press [enter] to exit.");

                        Console.ReadLine();
                    }
                }

                return;
            }

            HostFactory.Run(x =>
            {
                x.Service<Kernel>(s =>
                {
                    s.ConstructUsing(
                        z =>
                            new Kernel(GraphiteConfiguration.Instance,
                                GraphiteSystemConfiguration.Instance));
                    s.WhenStarted(k => k.Start());
                    s.WhenStopped(k => k.Stop());
                    s.WhenPaused(k => k.Stop());
                    s.WhenContinued(k => k.Start());
                })
                .StartAutomatically()
                .RunAsLocalSystem();
                x.SetServiceName("GraphiteSystemMonitoring");
                x.SetDisplayName("Graphite System Metrics Service");
                x.SetDescription("Publishes metrics to statsd/graphite");
            });
        }
    }
}
