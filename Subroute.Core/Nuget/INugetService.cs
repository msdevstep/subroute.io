using NuGet;
using Subroute.Core.Data;
using Subroute.Core.Models.Compiler;
using System;
using System.Threading.Tasks;

namespace Subroute.Core.Nuget
{
    public interface INugetService
    {
        Task DownloadPackageAsync(Dependency dependency);
        Task<NugetPackage[]> ResolveDependenciesAsync(Dependency dependency);
        Task<PagedCollection<NugetPackage>> SearchPackagesAsync(string term, int? skip = null, int? take = null);
    }
}