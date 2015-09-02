using Deployer.Service.Contracts;
using log4net;
using Quartz;

namespace Deployer.Service.Jobs.TeamCityPolling
{
    [DisallowConcurrentExecution]
    public class TeamCityPollingJob : IJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (TeamCityPollingJob));

        public ILogicLayer Polling { get; set; }

        public void Execute(IJobExecutionContext context)
        {
            Log.Info("Job executing.");

            Polling.Run();

            Log.Info("Job executed.");
        }
    }
}
