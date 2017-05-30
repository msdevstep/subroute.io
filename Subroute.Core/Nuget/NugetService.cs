using NuGet;
using Subroute.Core.Data;
using Subroute.Core.Exceptions;
using System;
using System.Linq;

namespace Subroute.Core.Nuget
{
    public class NugetService : INugetService
    {
        private readonly IPackageRepository _PackageRepository = null;

        public static string[] TargetFrameworks = new[] { ".NETFramework,Version=v4.5", ".NETFramework,Version=v4.0" };

        public NugetService()
        {
            _PackageRepository = PackageRepositoryFactory.Default.CreateRepository(Settings.NugetPackageUri);
        }

        public NugetPackage DownloadPackage(string id, Version version)
        {
            // Attempt to locate package by ID and Version in the nuget package repository.
            var sourcePackage = _PackageRepository.FindPackage(id, new SemanticVersion(version));

            // When no package was found, it could be a network error or the package was unlisted. Throw exception.
            if (sourcePackage == null)
                throw new NotFoundException($"Unable to locate package. Package ID: {id}, Version: {version.ToString(3)}.");

            // When the package is located, extract its contents into the standard local directory format.
            sourcePackage.ExtractContents(new PhysicalFileSystem(Settings.NugetPackageDirectory), $"{id}.{version.ToString(3)}");
            
            // To simplify getting the package details, we'll return the mapped package details.
            return NugetPackage.Map(sourcePackage);
        }

        public NugetPackage[] ResolveDependencies(string id, SemanticVersion version)
        {
            // Find the actual nuget package from the gallery to get its dependencies.
            var package = _PackageRepository.FindPackage(id, version);

            if (package == null)
                throw new NotFoundException($"Unable to locate package. Package ID: {id}, Version: {version.ToFullString()}.");

            // Recursively locate any additional nuget dependencies for this package that their
            // packages, and combine into a single output array.
            return new[] { NugetPackage.Map(package) }
                .Concat(ResolveDependencies(package))
                .ToArray();
        }

        private NugetPackage[] ResolveDependencies(IPackage package)
        {
            // Find nuget only dependencies.
            var dependencies = package.DependencySets
                .Where(ds => ds.TargetFramework == null && !ds.SupportedFrameworks.Any())
                .SelectMany(ds => ds.Dependencies)
                .ToArray();

            if (!dependencies.Any())
                return new NugetPackage[0];

            // Find actual packages in nuget gallery by IVersionSpec.
            var foundDependencies = dependencies
                .Select(p => _PackageRepository.FindPackage(p.Id, p.VersionSpec, true, true))
                .ToArray();

            // Determine which packages we could not locate.
            var missing = dependencies.Where(d => !foundDependencies.Any(fd => fd.Id == d.Id)).ToArray();

            if (missing.Any())
                throw new NotFoundException($"Unable to locate dependent packages: {string.Join(", ", missing.Select(m => m.Id))}");

            // Determine if any of these dependencies have other nuget dependencies.
            return foundDependencies
                .Select(NugetPackage.Map)
                .Concat(foundDependencies.SelectMany(fd => ResolveDependencies(fd)))
                .ToArray();
        }

        public PagedCollection<NugetPackage> SearchPackages(string keyword, int? skip = null, int? take = null)
        {
            // Lets apply a guard-rail to ensure the client doesn't request too much data.
            if (take.GetValueOrDefault() > 100 || !take.HasValue)
                take = 100;

            // Create source query to filter packages by keyword and target framework (include pre-releases).
            var query = _PackageRepository.Search(keyword, TargetFrameworks, true);

            // We'll use the PagedCollection class to return critical paging data back to the client.
            // Before we apply paging we'll need to get a total count back from the package store.
            // We will also apply the rest of the paging data for return to the client. In the event
            // that we don't have a page size (take), we'll use the total count since everything
            // will be returned.
            var result = new PagedCollection<NugetPackage>()
            {
                TotalCount = query.Count(),
                Skip = skip.GetValueOrDefault(),
                Take = take.GetValueOrDefault()
            };

            // Materialize the data from the nuget package repository.
            var packages = query
                .Skip(skip.GetValueOrDefault())
                .Take(take.GetValueOrDefault())
                .ToArray();

            // Make the actual request to the package store to get the current page of packages.
            // Skip and take will always be applied since we don't want to return an unbounded result set.
            // We'll be materializing the data, then projecting the data as a new type that is serializable.
            result.Results = packages.Select(NugetPackage.Map).ToArray();
            
            return result;
        }
    }
}
