using NuGet;
using Subroute.Core.Data;
using Subroute.Core.Models.Compiler;
using System;

namespace Subroute.Core.Nuget
{
    public interface INugetService
    {
        string DownloadPackage(NugetPackage package);
        NugetPackage[] ResolveDependencies(Dependency dependency);
        PagedCollection<NugetPackage> SearchPackages(string keyword, int? skip = null, int? take = null);
    }
}