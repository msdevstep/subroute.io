using NuGet;
using System.Linq;

namespace Subroute.Core.Nuget
{
    public class NugetService : INugetService
    {
        private readonly IPackageRepository _PackageRepository = null;

        public NugetService()
        {
            _PackageRepository = PackageRepositoryFactory.Default.CreateRepository(Settings.NugetPackageUri);
        }

        public NugetPackage[] SearchPackages(string keyword, int? skip = null, int? take = null)
        {
            var query = _PackageRepository.Search(keyword, new[] { ".NETFramework,Version=v4.5", ".NETFramework,Version=v4.0" }, true);

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            var materialized = query.ToArray();

            if (materialized.Any())
            {
                var first = materialized.First();
                first.ExtractContents(new PhysicalFileSystem("/"), @"C:\Packages\" + first.Id);
            }

            return materialized
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
