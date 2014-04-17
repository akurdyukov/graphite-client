using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using Graphite.Configuration;
using Graphite.System.Configuration;
using Topshelf;

namespace Graphite.System
{
    public static class Program
    {
        public static void Main(params string[] parameter)
        {

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
                });
                x.RunAsLocalSystem();
                x.SetServiceName("FE-Metrics");
                x.SetDisplayName("FlowEnergy Metrics Service");
                x.SetDescription("Publishes metrics to statsd/graphite");
            });
        }
    }
}
