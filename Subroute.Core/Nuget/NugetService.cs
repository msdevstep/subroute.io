using NuGet;
using System.Linq;

namespace Subroute.Core.Nuget
{
    public class NugetService : INugetService
    {
        public NugetPackage[] SearchPackages(string keyword, int? skip = null, int? take = null)
        {
            var repository = PackageRepositoryFactory.Default.CreateRepository(Settings.NugetPackageUri);
            var query = repository.Search(keyword, false);

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            return query
                .ToArray()
                .Select(p => new NugetPackage
                {
                    Id = p.Id,
                    Title = p.Title,
                    Summary = p.Summary,
                    Description = p.Description,
                    DownloadCount = p.DownloadCount,
                    IconUrl = p.IconUrl,
                    ProjectUrl = p.ProjectUrl,
                    LicenseUrl = p.LicenseUrl,
                    Tags = p.Tags,
                    Version = p.Version?.ToString(),
                    PublishedOn = p.Published,
                    Owners = p.Owners,
                    MinClientVersion = p.MinClientVersion?.ToString(),
                    Language = p.Language,
                    IsLatestVersion = p.IsLatestVersion,
                    Authors = p.Authors
                }).ToArray();
        }
    }
}
