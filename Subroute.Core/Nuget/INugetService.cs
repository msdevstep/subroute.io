using NuGet;

namespace Subroute.Core.Nuget
{
    public interface INugetService
    {
        NugetPackage[] SearchPackages(string keyword, int? skip = null, int? take = null);
    }
}