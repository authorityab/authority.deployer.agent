using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using Deployer.Api.Classes;
using Deployer.Api.Controllers;
using Deployer.Api.Models;
using Deployer.Api.Services;
using Deployer.Service;
using Newtonsoft.Json;

namespace SoftResource.Deployer.Service
{
    public partial class Service : ServiceBase
    {
        private Thread _buildsWorker;


        private readonly AutoResetEvent _stopRequest = new AutoResetEvent(false);
        private ITeamCityService _tcService;
        private List<Build> lastBuilds;// = new List<Build>();

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            Debugger.Launch();

            var username = WebConfigurationManager.AppSettings["TeamCityUsername"];
            var password = WebConfigurationManager.AppSettings["TeamCityPassword"];
            var url = WebConfigurationManager.AppSettings["TeamCityUrl"];

            _tcService = new TeamCityService(username, password, url);

            _buildsWorker = new Thread(GetBuilds);
            _buildsWorker.Start();


            //ITeamCityService tcService = new TeamCityService();

            //var builds = tcService.GetAllBuilds();
        }

        protected override void OnStop()
        {
            _stopRequest.Set();
            _buildsWorker.Join();
        }

        private void GetBuilds(object arg)
        {
            while (true)
            {
                if (_stopRequest.WaitOne(10000))
                    return;

                var builds = _tcService.GetAllBuilds();
                if (lastBuilds != null)
                {
                    if (!lastBuilds.SequenceEqual(builds, new BuildComparer()))
                    {
                        if (builds.Any(x => x.Status == BuildStatus.Failure.ToString().ToUpper()))
                        {
                            // Get latest failed build, send to node;
                            var latestFailedBuild = _tcService.GetLatestFailedBuild();
                            PostLatestFailedBuild(latestFailedBuild);
                        }
                        // Send builds to node
                        PostBuilds(builds);
                    }
                }
                //else
                //{
                //    // Get latest failed build, send to node;
                //    var latestFailedBuild = _tcService.GetLatestFailedBuild();
                //    PostLatestFailedBuild(latestFailedBuild);

                //    // Send builds to node
                //    PostBuilds(builds);
                //}

                // Send latest build to node, builds.first()
                if (builds.Count > 0)
                {
                    PostLatestBuild(builds.First());
                }

                lastBuilds = builds;
            }
        }

        // TODO: Move to Deployer.Api "NodeService"
        public static bool PostBuilds(List<Build> builds)
        {
            using (var client = new HttpClient())
            {
                var response = client.PostAsync("http://192.168.21.70:3000/api/setbuilds",
                    new StringContent(JsonConvert.SerializeObject(builds), Encoding.UTF8, "application/json")).Result;

                if (response.IsSuccessStatusCode)
                {
                    dynamic content = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                    var isSuccess = bool.Parse(content.success.ToString());

                    return isSuccess;
                }

                return false;
            }
        }

        // TODO: Move to Deployer.Api "NodeService"
        public static bool PostLatestBuild(Build build)
        {
            using (var client = new HttpClient())
            {
                var response = client.PostAsync("http://192.168.21.70:3000/api/setlatestbuild",
                    new StringContent(JsonConvert.SerializeObject(build), Encoding.UTF8, "application/json")).Result;

                if (response.IsSuccessStatusCode)
                {
                    dynamic content = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                    var isSuccess = bool.Parse(content.success.ToString());

                    return isSuccess;
                }

                return false;
            }
        }

        // TODO: Move to Deployer.Api "NodeService"
        public static bool PostLatestFailedBuild(Build build)
        {
            using (var client = new HttpClient())
            {
                var response = client.PostAsync("http://192.168.21.70:3000/api/setlatestfailedbuild",
                    new StringContent(JsonConvert.SerializeObject(build), Encoding.UTF8, "application/json")).Result;

                if (response.IsSuccessStatusCode)
                {
                    dynamic content = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                    var isSuccess = bool.Parse(content.success.ToString());

                    return isSuccess;
                }

                return false;
            }
        }
    }
}
