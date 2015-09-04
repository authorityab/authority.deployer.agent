using System.Collections.Generic;
using Authority.Deployer.Api.Models;

namespace Authority.Deployer.Service.Classes
{
    public class BuildComparer : IEqualityComparer<Build>
    {
        public bool Equals(Build x, Build y)
        {
            return (x.ProjectId.Equals(y.ProjectId))
                   && (x.ProjectName.Equals(y.ProjectName)
                       && (x.StepName.Equals(y.StepName))
                       && (x.Status.Equals(y.Status)));
        }

        public int GetHashCode(Build obj)
        {
            return obj.GetHashCode();
        }
    }
}
