using System;
using System.Collections.Generic;
using System.Linq;
using Authority.Deployer.Api.Classes;
using Authority.Deployer.Api.Models;
using Authority.Deployer.Api.Services;
using Authority.Deployer.Api.Services.Contracts;
using Authority.Deployer.Service.Classes;
using Authority.Deployer.Service.Contracts;
using log4net;

namespace Authority.Deployer.Service.Jobs.TeamCityPolling
{
    public class TeamCityPoller : ILogicLayer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TeamCityPoller));
        private readonly ITeamCityService _tcService;
        private readonly INodeService _nodeService;
        private List<Build> _lastBuilds;
        private bool _isSuccess;

        public TeamCityPoller()
        {
            // TODO: Fix Dependecy Injection on these services.
            _tcService = new TeamCityService();
            _nodeService = new NodeService();
        }

        public void Run()
        {
            Log.Info("Polling started.");

            try
            {
                var builds = _tcService.GetAllBuilds();

                if (_lastBuilds != null)
                {
                    if (!_lastBuilds.SequenceEqual(builds, new BuildComparer()))
                    {
                        Log.Info("Found changes in builds, posting to node");

                        if (builds.Any(x => x.Status == BuildStatus.Failure.ToString().ToUpper()))
                        {
                            Log.Info("Found failed build, posting to node");

                            // Get latest failed build, send to node;
                            var latestFailedBuild = _tcService.GetLatestFailedBuild();
                            _nodeService.PostLatestFailedBuild(latestFailedBuild);
                        }

                        _isSuccess = _nodeService.PostBuilds(builds);
                    }
                }

                if (_lastBuilds == null && !_isSuccess)
                {
                    Log.Info("Last push to node did not succeed, posting again");

                    // Send builds to node
                    _isSuccess = _nodeService.PostBuilds(builds);
                }

                // Send latest build to node, builds.first()
                if (builds.Count > 0)
                {
                    Log.Info("Posting latest build to node");
                    _nodeService.PostLatestBuild(builds.First());
                }

                if (_isSuccess && builds.Count > 0)
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
            Log.Info("TeamCity polling job disposed.");
        }
    }


}
