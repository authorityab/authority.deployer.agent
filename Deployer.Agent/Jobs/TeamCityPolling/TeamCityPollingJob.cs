using Authority.Deployer.Agent.Contracts;
using log4net;
using Quartz;

namespace Authority.Deployer.Agent.Jobs.TeamCityPolling
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
