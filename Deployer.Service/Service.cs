using System.Configuration;
using Atlas;
using Deployer.Service.Jobs.TeamCityPolling;
using log4net;
using Quartz;
using Quartz.Spi;

namespace Deployer.Service
{
    public class Service : IAmAHostedProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Service));

        public IScheduler Scheduler { get; set; }

        public IJobFactory JobFactory { get; set; }

        public IJobListener JobListener { get; set; }

        public void Start()
        {
            Log.Info("Deployer service starting.");

            var job = JobBuilder.Create<TeamCityPollingJob>()
                .WithIdentity("TeamCityPollingJob", "DeployerService")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("TeamCityPollingTrigger", "DeployerService")
                .WithCronSchedule(ConfigurationManager.AppSettings["TcJobCronExp"])
                .ForJob("TeamCityPollingJob", "DeployerService")
                
                .Build();

            Scheduler.JobFactory = JobFactory;
            Scheduler.ScheduleJob(job, trigger);
            Scheduler.ListenerManager.AddJobListener(JobListener);

            Scheduler.Start();

            Log.Info("Deployer service started.");
        }

        public void Stop()
        {
            Log.Info("Deployer service stopping.");

            Scheduler.Shutdown();

            Log.Info("Deployer service stopped.");
        }

        public void Resume()
        {
            Log.Info("Deployer service resuming.");

            Scheduler.ResumeAll();
            
            Log.Info("Deployer service resumed.");
        }

        public void Pause()
        {
            Log.Info("Deployer service pausing.");

            Scheduler.PauseAll();

            Log.Info("Deployer service paused.");
        }
    }
}
