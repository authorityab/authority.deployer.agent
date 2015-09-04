using System;
using System.Linq;
using Atlas;
using Authority.Deployer.Service.Classes;
using Autofac;
using log4net;
using log4net.Config;

namespace Authority.Deployer.Service
{
    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                XmlConfigurator.Configure();

                var configuration = Host.UseAppConfig<Service>()
                    //.AllowMultipleInstances()
                    .WithRegistrations(p => p.RegisterModule(new AutofacConfig()));
                    

                if (args != null && args.Any())
                    configuration = configuration.WithArguments(args);

                Host.Start(configuration);
            }
            catch (Exception ex)
            {
                Log.Fatal("Exception during startup.", ex);
                Console.ReadLine();
            }
        }
    }
}
