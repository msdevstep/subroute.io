using NuGet;
using Subroute.Core.Data;
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

        public PagedCollection<NugetPackage> SearchPackages(string keyword, int? skip = null, int? take = null)
        {
            // Lets apply a guard-rail to ensure the client doesn't request too much data.
            if (take.GetValueOrDefault() > 100 || !take.HasValue)
                take = 100;

            // Create source query to filter packages by keyword and target framework (include pre-releases).
            var query = _PackageRepository.Search(keyword, new[] { ".NETFramework,Version=v4.5", ".NETFramework,Version=v4.0" }, true);

            // We'll use the PagedCollection class to return critical paging data back to the client.
            var result = new PagedCollection<NugetPackage>();

            // Before we apply paging we'll need to get a total count back from the package store.
            // We will also apply the rest of the paging data for return to the client. In the event
            // that we don't have a page size (take), we'll use the total count since everything
            // will be returned.
            result.TotalCount = query.Count();
            result.Skip = skip.GetValueOrDefault();
            result.Take = take.GetValueOrDefault();

            // Apply paging to limit the total number of packages we are pulling from the package store.
            if (skip.HasValue)
                query = query.Skip(skip.Value);

            // Make the actual request to the package store to get the current page of packages.
            // Take will always be applied since we don't want to return an unbounded result set.
            // We'll be materializing the data, then projecting the data as a new type that is serializable.
            result.Results = query
                .Take(take.GetValueOrDefault())
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

            return result;
        }
    }
}
