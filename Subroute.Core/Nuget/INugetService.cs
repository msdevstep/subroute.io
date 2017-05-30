using NuGet;
using Subroute.Core.Data;
using System;

namespace Subroute.Core.Nuget
{
    public interface INugetService
    {
        NugetPackage DownloadPackage(string id, Version version);
        NugetPackage[] ResolveDependencies(string id, SemanticVersion version);
        PagedCollection<NugetPackage> SearchPackages(string keyword, int? skip = null, int? take = null);
    }
}