using System;
using System.Collections.Generic;
using System.Linq;
using Authority.Deployer.Agent.Contracts;
using Authority.Deployer.Api.Services;
using Authority.Deployer.Api.Services.Contracts;
using log4net;
using Build = Authority.Deployer.Api.Models.Build;
using BuildStatus = Authority.Deployer.Api.Classes.BuildStatus;

namespace Authority.Deployer.Agent.Jobs.TeamCityPolling
{
    public class TeamCityPoller : ILogicLayer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TeamCityPoller));
        private readonly ITeamCityService _tcService;
        private readonly INodeService _nodeService;
        private List<Build> _lastBuilds;
        private Build _lastFailedBuild;
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
                    var a = _lastBuilds.OrderBy(x => x.FinishDate).ToList();
                    var b = builds.OrderBy(x => x.FinishDate).ToList();
                    
                    Log.Info("Last builds is not null");
                    if (!a.SequenceEqual(b))
                    {
                        for (var i = 0; i < a.Count; i++)
                        {
                            if (!a[i].Equals(b[i]))
                            {
                                Log.Debug("Item not equal");
                                Log.Debug($"{a[i].ProjectName} <--> {b[i].ProjectName}");
                                Log.Debug($"{a[i].ProjectId} <--> {b[i].ProjectId}");
                                Log.Debug($"{a[i].StepName} <--> {b[i].StepName}");
                                Log.Debug($"{a[i].Status} <--> {b[i].Status}");
                                Log.Debug("-----------------------------------");
                            }
                        }

                        Log.Info("Found changes in builds, posting to node");
                        
                        _isSuccess = _nodeService.PostBuilds(builds);
                        Log.Info("Post builds SUCCESS: " + _isSuccess);

                        //_tcService.PutBuildsInCache(builds);
                    }

                    CheckForFailedBuilds(builds);
                }

                if (_lastBuilds == null && !_isSuccess)
                {
                    Log.Info("Last push to node did not succeed, posting again");

                    ////PostLatestFailedBuild();

                    // Send builds to node
                    _isSuccess = _nodeService.PostBuilds(builds);

                    //_tcService.PutBuildsInCache(builds);
                    Log.Info("Post builds SUCCESS: " + _isSuccess);
                }

                // Send latest build to node, builds.first()
                if (builds.Count > 0)
                {
                    Log.Info("Posting latest build to node");
                    _nodeService.PostLatestBuild(builds.First());
                }

                if (_isSuccess && builds.Count > 0)
                {
                    Log.Info("Replace last builds");
                    _lastBuilds = builds;
                }
            }
            catch (Exception e)
            {
                Log.Error("TeamCity polling job failed. ", e);
            }

            Log.Info("Polling finished.");
        }

     

        private void CheckForFailedBuilds(IEnumerable<Build> builds)
        {
            var failedBuilds = builds.Where(x => x.Status == BuildStatus.Failure.ToString().ToUpper()).ToList();
            if (failedBuilds.Any())
            {
                // Get latest failed build, send to node;
                var latestFailedBuild = _tcService.GetLatestFailedBuild();

                if (_lastFailedBuild == null || !_lastFailedBuild.Equals(latestFailedBuild)
                    || _lastFailedBuild.FinishDate != latestFailedBuild.FinishDate
                    || _lastFailedBuild.LastModifiedBy != latestFailedBuild.LastModifiedBy)
                {
                    Log.Info("Found failed build, posting to node");
                    if (_nodeService.PostLatestFailedBuild(latestFailedBuild))
                    {
                        _lastFailedBuild = latestFailedBuild;
                    }
                }

                //TODO: Maybe
                //_nodeService.PostFailedBuilds(failedBuilds);
            }
            else
            {
                _lastFailedBuild = null;
            }
        }

        public void Dispose()
        {
            Log.Info("TeamCity polling job disposed.");
        }
    }
}
