using Graphite.Configuration;
using Graphite.System.Configuration;
using Topshelf;

namespace Graphite.System
{
    public static class Program
    {
        public const string ServiceName = "GraphiteSystemMonitor";
        public const string DisplayName = "GraphiteSystemMonitor";
        public const string Description = "Publishes metrics to statsd/graphite";

        public static void Main(params string[] parameter)
        {
            HostFactory.Run(x =>
            {
                // add the service
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
                x.SetServiceName(ServiceName);
                x.SetDisplayName(DisplayName);
                x.SetDescription(Description);
            });
        }
    }
}
