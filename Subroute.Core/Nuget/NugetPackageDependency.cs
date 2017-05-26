using NuGet;
using System;

namespace Subroute.Core.Nuget
{
    public class NugetPackageDependency
    {
        public string Id { get; set; }
        public Version Version { get; set; }

        public static Func<PackageDependency, NugetPackageDependency> Map => pd => new NugetPackageDependency
        {
            Id = pd.Id,
            Version = pd.VersionSpec?.MaxVersion?.Version
        };
    }
}
