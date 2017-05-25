using NuGet;
using Subroute.Core.Data;

namespace Subroute.Core.Nuget
{
    public interface INugetService
    {
        PagedCollection<NugetPackage> SearchPackages(string keyword, int? skip = null, int? take = null);
    }
}