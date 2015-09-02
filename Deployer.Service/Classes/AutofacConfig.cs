using System.Reflection;
using Atlas;
using Autofac;
using Deployer.Service.Contracts;
using Deployer.Service.Jobs.TeamCityPolling;
using Deployer.Service.Scheduling;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Module = Autofac.Module;

namespace Deployer.Service.Classes
{
    public class AutofacConfig : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterModule(new PollingModule());

            LoadQuartz(builder);
            LoadWindowsService(builder);
            //LoadExternalServices(builder);
            LoadLogicLayers(builder);


        }

        private static void LoadQuartz(ContainerBuilder builder)
        {
            //builder.Register(c => new TeamCityService()).As<ITeamCityService>();
            //builder.Register(c => new NodeService()).As<INodeService>();

            builder.Register(c => new StdSchedulerFactory().GetScheduler())
                .As<IScheduler>()
                .InstancePerLifetimeScope();

            builder.Register(c => new JobFactory(ContainerProvider.Instance.ApplicationContainer))
                .As<IJobFactory>();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(p => typeof(IJob).IsAssignableFrom(p))
                .PropertiesAutowired();

            builder.Register(c => new JobListener(ContainerProvider.Instance))
                .As<IJobListener>();
        }

        private static void LoadWindowsService(ContainerBuilder builder)
        {
            builder.RegisterType<Service>()
                .As<IAmAHostedProcess>()
                .PropertiesAutowired();
        }

        //private static void LoadExternalServices(ContainerBuilder builder)
        //{
        //    builder.RegisterType<TeamCityService>()
        //        .As<ITeamCityService>();
        //        //.InstancePerLifetimeScope();

        //    builder.RegisterType<NodeService>()
        //        .As<INodeService>();
        //    //.InstancePerLifetimeScope();
        //}

        private static void LoadLogicLayers(ContainerBuilder builder)
        {
            builder.RegisterType<LogicLayer>()
                .As<ILogicLayer>()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            //builder.Register(c =>
            //    new LogicLayer(c.Resolve<ITeamCityService>(), c.Resolve<INodeService>()))
            //    .As<ILogicLayer>();


            //builder.Register((c, p) =>
            //    new TeamCityService(tcUsername, tcPassword, tcUrl))
            //    .As<ITeamCityService>()
            //    .InstancePerLifetimeScope();
        }
    }
}
