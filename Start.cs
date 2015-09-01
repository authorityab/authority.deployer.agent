using System.ServiceProcess;

namespace Deployer.Service
{
    static class Start
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
            { 
                new SoftResource.Deployer.Service.Service() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
