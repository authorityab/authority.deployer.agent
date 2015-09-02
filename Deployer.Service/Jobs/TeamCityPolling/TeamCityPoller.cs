using System;
using System.Collections.Generic;
using System.Linq;
using Deployer.Api.Classes;
using Deployer.Api.Models;
using Deployer.Api.Services;
using Deployer.Api.Services.Contracts;
using Deployer.Service.Classes;
using Deployer.Service.Contracts;
using log4net;

namespace Deployer.Service.Jobs.TeamCityPolling
{
    public class LogicLayer : ILogicLayer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LogicLayer));

        public ITeamCityService TcService { get; set; }

        public INodeService NodeService { get; set; }

        private List<Build> _lastBuilds;

        public LogicLayer()
        {
            // TODO: Fix Dependecy Injection on these services.
            TcService = new TeamCityService();
            NodeService = new NodeService();
        }

        public void Run()
        {
            Log.Info("Polling started.");

            try
            {
                var isSuccess = false;
                var builds = TcService.GetAllBuilds();

                if (_lastBuilds != null)
                {
                    if (!_lastBuilds.SequenceEqual(builds, new BuildComparer()))
                    {
                        if (builds.Any(x => x.Status == BuildStatus.Failure.ToString().ToUpper()))
                        {
                            // Get latest failed build, send to node;
                            var latestFailedBuild = TcService.GetLatestFailedBuild();
                            NodeService.PostLatestFailedBuild(latestFailedBuild);
                        }
                        // Send builds to node
                        isSuccess = NodeService.PostBuilds(builds);
                    }
                }

                // Send latest build to node, builds.first()
                if (builds.Count > 0)
                {
                    NodeService.PostLatestBuild(builds.First());
                }

                if (isSuccess && builds.Count > 0)
                {
                    _lastBuilds = builds;
                }
            }
            catch (Exception e)
            {
                Log.Error("TeamCity polling job failed. ", e);
            }

            Log.Info("Polling finished.");

        }

        public void Dispose()
        {
        }
    }


}
