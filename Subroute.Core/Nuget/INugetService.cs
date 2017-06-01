using NuGet;
using Subroute.Core.Data;
using Subroute.Core.Models.Compiler;
using System;
using System.Threading.Tasks;

namespace Subroute.Core.Nuget
{
    public interface INugetService
    {
        string DownloadPackage(NugetPackage package);
        NugetPackage[] ResolveDependencies(Dependency dependency);
        Task<PagedCollection<NugetPackage>> SearchPackagesAsync(string keyword, int? skip = null, int? take = null);
    }
}