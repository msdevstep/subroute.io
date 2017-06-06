using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using Subroute.Core.Data;
using Subroute.Core.Models.Compiler;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static NuGet.Protocol.Core.Types.Repository;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using NuGet.Common;
using NuGet.Versioning;
using Subroute.Core.Exceptions;
using NuGet.Protocol;
using System.Xml.Linq;

namespace Subroute.Core.Nuget
{
    public class NugetService : INugetService
    {
        private readonly ILogger _traceLogger = new TraceLogger();

        private NuGetFramework GetCurrentFramework()
        {
            string frameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
                .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                .Select(x => x.FrameworkName)
                .FirstOrDefault();
            return frameworkName == null
                ? NuGetFramework.AnyFramework
                : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());
        }

        public async Task DownloadPackageAsync(Dependency dependency)
        {
            var identity = new PackageIdentity(dependency.Id, new NuGetVersion(dependency.Version));
            var project = new FolderNuGetProject(Settings.NugetPackageDirectory);
            var settings = NuGet.Configuration.Settings.LoadDefaultSettings(Settings.NugetPackageDirectory);
            var provider = new SourceRepositoryProvider(settings, Provider.GetCoreV3());
            NuGetPackageManager packageManager = new NuGetPackageManager(provider, settings, Settings.NugetPackageDirectory)
            {
                PackagesFolderNuGetProject = project
            };
            bool allowPrereleaseVersions = true;
            bool allowUnlisted = false;
            ResolutionContext resolutionContext = new ResolutionContext(NuGet.Resolver.DependencyBehavior.Lowest, allowPrereleaseVersions, allowUnlisted, VersionConstraints.None);
            INuGetProjectContext projectContext = new NuGetProjectContext();
            IEnumerable<SourceRepository> sourceRepositories = provider.GetRepositories();
            await packageManager.InstallPackageAsync(packageManager.PackagesFolderNuGetProject,
                identity, resolutionContext, projectContext, sourceRepositories,
                Array.Empty<SourceRepository>(),
                CancellationToken.None);

            //// First we'll determine if the package has already been download. We only need to download the package
            //// once. Then every single subroute that references this package will already have it.
            //var folder = $"{package.Id}.{package.Version}";
            //var path = Path.Combine(Settings.NugetPackageDirectory, folder);

            //if (Directory.Exists(path))
            //    return path;

            //// Attempt to locate package by ID and Version in the nuget package repository.
            //var sourcePackage = _PackageRepository.FindPackage(package.Id, SemanticVersion.Parse(package.Version));

            //// When no package was found, it could be a network error or the package was unlisted. Throw exception.
            //if (sourcePackage == null)
            //    throw new NotFoundException($"Unable to locate package. Package ID: {package.Id}, Version: {package.Version}.");

            //// When the package is located, extract its contents into the standard local directory format.
            //sourcePackage.ExtractContents(new PhysicalFileSystem(Settings.NugetPackageDirectory), folder);

            //// To simplify getting the package details, we'll return the mapped package details.
            //return path;
        }

        public async Task<NugetPackage[]> ResolveDependenciesAsync(Dependency dependency)
        {
            // Get an instance of the provider used to get package result data from the NuGet gallery.
            var packageMetadataResource = await Provider.GetResourceAsync<PackageMetadataResource>();
            var identity = new PackageIdentity(dependency.Id, new NuGetVersion(dependency.Version));
            var packageMetadata = await packageMetadataResource.GetMetadataAsync(identity, _traceLogger, CancellationToken.None);

            if (packageMetadata == null)
                throw new NotFoundException($"Unable to locate package. Package ID: {dependency.Id}, Version: {dependency.Version}.");

            // Recursively locate any additional nuget dependencies for this package that their
            // packages, and combine into a single output array.
            return new[] { NugetPackage.Map(packageMetadata) }
                .Concat(await ResolveDependenciesAsync(packageMetadata))
                .Distinct(new NugetPackageComparer())
                .ToArray();
        }

        private async Task<NugetPackage[]> ResolveDependenciesAsync(IPackageSearchMetadata package)
        {
            // Dependency sets that have IsAny = true for TargetFramework are the NuGet package references.
            // Isolate them so we can get their metadata and resolve any dependencies they may have.
            var dependencies = package
                .DependencySets
                .Where((ds, i) => ds.TargetFramework.IsAny && i == 0)
                .SelectMany(ds => ds.Packages);

            // An instance of PackageMetadataResource is needed to get the full package 
            // metadata for each of the dependencies. Also create a NugetPackage list
            // to keep track of all the dependencies we resolve.
            var packageMetadataResource = await Provider.GetResourceAsync<PackageMetadataResource>();
            var results = new List<NugetPackage>();

            // Iterate over each dependency to request full metadata for each one.
            foreach (var dependency in dependencies)
            {
                // We'll use the dependencies VersionSpec to find the highest matching
                // version that will satisfy the dependency.
                var bestVersion = await GetHighestMatchingVersion(dependency);

                // Once we've identified which package version we want to load, we'll
                // need to load the full package metadata to convert to NugetPackage.
                // First we create a new PackageIdentity that will hold the ID and
                // Version that we intend to load. Then use the identity to load the
                // full metadata for the package.
                var dependencyIdentity = new PackageIdentity(dependency.Id, bestVersion);
                var dependencyPackage = await packageMetadataResource.GetMetadataAsync(dependencyIdentity, _traceLogger, CancellationToken.None);

                // In the case where we can't locate a dependency by its identity. We
                // should fail the whole sequence, as most likely it will not build.
                if (dependencyPackage == null)
                    throw new NotFoundException($"Unable to locate package. Package ID: {dependencyIdentity.Id}, Version: {dependencyIdentity.Version}.");

                // Get an enumerable of this current dependency and all of its dependencies.
                var result = new [] { NugetPackage.Map(dependencyPackage) }.Concat(await ResolveDependenciesAsync(dependencyPackage));

                // All the current dependency chain to the output results.
                results.AddRange(result);
            }

            return results.ToArray();
        }

        private async Task<NuGetVersion> GetHighestMatchingVersion(PackageDependency dependency)
        {
            var dependencyInfoResource = await Provider.GetResourceAsync<DependencyInfoResource>();
            var dependencyInfo = await dependencyInfoResource.ResolvePackages(dependency.Id, GetCurrentFramework(), new TraceLogger(), CancellationToken.None);

            return dependencyInfo
                .Select(x => x.Version)
                .Where(x => x != null && (dependency.VersionRange == null || dependency.VersionRange.Satisfies(x)))
                .DefaultIfEmpty()
                .Max();
        }

        /// <summary>
        /// Searches the NuGet gallery for package data and returns a page of data at a time.
        /// </summary>
        /// <param name="term">Search term used to filter the package result data.</param>
        /// <param name="skip">Starting index of the page of data to return.</param>
        /// <param name="take">Number of records to include in the result data.</param>
        /// <returns>Returns an instance of <see cref="PagedCollection{NugetPackage}"/> containing paging data and package results.</returns>
        public async Task<PagedCollection<NugetPackage>> SearchPackagesAsync(string term, int? skip = null, int? take = null)
        {
            // Lets apply a guard-rail to ensure the client doesn't request too much data.
            if (take.GetValueOrDefault() > 100 || !take.HasValue)
                take = 100;
            
            // Get an instance of the provider used to get package result data from the NuGet gallery.
            var searchResource = await Provider.GetResourceAsync<PackageSearchResourceEnhancedV3>();
            var searchFilter = new SearchFilter(false, SearchFilterType.IsLatestVersion);
            var searchMetadata = await searchResource.SearchPageableAsync(term,
                searchFilter,
                skip.GetValueOrDefault(),
                take.GetValueOrDefault(),
                new TraceLogger(),
                CancellationToken.None);

            var package = searchMetadata.Results.FirstOrDefault();

            if (package != null)
            {
                var dependency = new Dependency
                {
                    Id = package.Identity.Id,
                    Version = package.Identity.Version.ToFullString()
                };
                var dependents = await ResolveDependenciesAsync(dependency);
            }

            // Extract and return paging data and search results.
            return new PagedCollection<NugetPackage>
            {
                Skip = skip.GetValueOrDefault(),
                Take = take.GetValueOrDefault(),
                TotalCount = searchMetadata.TotalCount,
                Results = searchMetadata.Results
                    .Select(m => NugetPackage.Map(m))
                    .ToArray()
            };
        }
    }
}
