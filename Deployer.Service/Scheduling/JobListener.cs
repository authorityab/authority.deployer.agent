using System;
using Atlas;
using Quartz;

namespace Deployer.Service.Scheduling
{
    public class JobListener : IJobListener
    {
        public string Name { get; private set; }

        private readonly IContainerProvider _provider;

        private IUnitOfWorkContainer _container;

        public JobListener(IContainerProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            _provider = provider;
            Name = "JobListener";
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            _container = _provider.CreateUnitOfWork();
            _container.InjectUnsetProperties(context.JobInstance);
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            _container.Dispose();
        }
    }
}
