using NuGet;
using Subroute.Core.Data;
using Subroute.Core.Models.Compiler;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Subroute.Core.Nuget
{
    public interface INugetService
    {
        Task DownloadPackageAsync(RoutePackage package);
        Task DownloadPackageAsync(Dependency dependency);
        PackageReference[] GetPackageReferences(Dependency dependency);
        Task<NugetPackage[]> ResolveAllDependenciesAsync(IEnumerable<Dependency> dependencies);
        Task<NugetPackage[]> ResolveDependenciesAsync(Dependency dependency);
        Task<PagedCollection<NugetPackage>> SearchPackagesAsync(string term, int? skip = null, int? take = null);
    }
}