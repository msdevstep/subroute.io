using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using Subroute.Core.Data;
using Subroute.Core.Models.Compiler;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Versioning;
using Subroute.Core.Exceptions;
using NuGet.Protocol;
using Subroute.Core.Extensions;

namespace Subroute.Core.Nuget
{
    /// <summary>
    /// Contains functionality for interacting with the NuGet package gallery.
    /// </summary>
    public class NugetService : INugetService
    {
        private readonly ILogger _traceLogger = new TraceLogger();
        private readonly FolderNuGetProject _packageFolder;
        private readonly NuGetFramework _currentFramework;

        /// <summary>
        /// Constructs an instance of <see cref="NugetService"/> enabling access to the NuGet gallery.
        /// </summary>
        public NugetService()
        {
            _packageFolder = new FolderNuGetProject(Settings.NugetPackageDirectory);
            _currentFramework = GetCurrentFramework();
        }

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

        private async Task<NuGetVersion> GetHighestMatchingVersion(PackageDependency dependency)
        {
            var dependencyInfoResource = await Repository.Provider.GetResourceAsync<DependencyInfoResource>();
            var dependencyInfo = await dependencyInfoResource.ResolvePackages(dependency.Id, _currentFramework, new TraceLogger(), CancellationToken.None);

            return dependencyInfo
                .Select(x => x.Version)
                .Where(x => x != null && (dependency.VersionRange == null || dependency.VersionRange.Satisfies(x)))
                .DefaultIfEmpty()
                .Max();
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
            var packageMetadataResource = await Repository.Provider.GetResourceAsync<PackageMetadataResource>();
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

        public async Task<PackageReference[]> GetPackageReference(Dependency dependency)
        {
            var identity = dependency.ToPackageIdentity();
            var packageFilePath = _packageFolder.GetInstalledPackageFilePath(identity);

            if (!packageFilePath.HasValue())
                return new PackageReference[0];

            var archiveReader = new PackageArchiveReader(packageFilePath, null, null);
            var referenceGroup = GetMostCompatibleGroup(_currentFramework, archiveReader.GetReferenceItems());
            var references = new List<PackageReference>();

            if (referenceGroup != null)
            {
                foreach (var group in referenceGroup.Items)
                {
                    var qualified = _packageFolder.GetInstalledPath(identity);
                    var combined = Path.Combine(qualified, group.Replace("/", "\\"));
                    var directory = Path.GetDirectoryName(combined);
                    var filename = Path.GetFileNameWithoutExtension(combined);
                    var documentation = Path.Combine(directory, string.Concat(filename, ".xml"));
                    var reference = new PackageReference
                    {
                        AssemblyPath = combined,
                        DocumentationPath = File.Exists(documentation) ? documentation : null
                    };

                    references.Add(reference);
                }
            }

            return references.ToArray();
        }

        /// <summary>
        /// Downloads and extracts the specified dependency to the NuGet package folder.
        /// </summary>
        /// <param name="dependency">Dependency indicating the specific Id and Version of the package to download.</param>
        public async Task DownloadPackageAsync(Dependency dependency)
        {
            // Use V3 providers to get an instance of the NuGetPackageManager. We'll use
            // defaults for everything as we are doing a simple package download to a folder.
            var identity = dependency.ToPackageIdentity();
            var settings = NuGet.Configuration.Settings.LoadDefaultSettings(Settings.NugetPackageDirectory);
            var provider = new SourceRepositoryProvider(settings, Repository.Provider.GetCoreV3());
            var packageManager = new NuGetPackageManager(provider, settings, Settings.NugetPackageDirectory)
            {
                PackagesFolderNuGetProject = _packageFolder
            };
            
            // We'll create an instance of ResolutionContext using the standard resolution behavior.
            var resolutionContext = new ResolutionContext(NuGet.Resolver.DependencyBehavior.Lowest, true, true, VersionConstraints.None);
            var projectContext = new NuGetProjectContext();
            var sourceRepositories = provider.GetRepositories();

            // Use the package manager to download an install the specified dependencies.
            await packageManager.InstallPackageAsync(packageManager.PackagesFolderNuGetProject,
                identity, resolutionContext, projectContext, sourceRepositories,
                Array.Empty<SourceRepository>(),
                CancellationToken.None);
        }

        /// <summary>
        /// Resolves all possible packages from the provided array of dependencies and
        /// returns an array of packages with no duplicates.
        /// </summary>
        /// <param name="dependencies">Dependencies used to resolve packages.</param>
        /// <returns>Array of <see cref="NugetPackage"/> containing all resolved dependencies.</returns>
        public async Task<NugetPackage[]> ResolveAllDependenciesAsync(IEnumerable<Dependency> dependencies)
        {
            var packages = new List<NugetPackage>();

            // Iterate over each provided dependencies to resolve all child dependenceis.
            // Add all resolved packages to the result list to be deduplicated.
            foreach (var dependency in dependencies)
                packages.AddRange(await ResolveDependenciesAsync(dependency));

            // Deduplicate package list using the NugetPackageComparer with compares
            // each package using its Id and Version.
            return packages
                .Distinct(new NugetPackageComparer())
                .ToArray();
        }

        /// <summary>
        /// Resolves all package dependencies for the specified dependency and returns
        /// an array of packages with no duplicates.
        /// </summary>
        /// <param name="dependency">Dependency used to resolve packages.</param>
        /// <returns>Array of <see cref="NugetPackage"/> containing all resolved dependencies.</returns>
        public async Task<NugetPackage[]> ResolveDependenciesAsync(Dependency dependency)
        {
            // Get an instance of the provider used to get package result data from the NuGet gallery.
            var packageMetadataResource = await Repository.Provider.GetResourceAsync<PackageMetadataResource>();
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
            var searchResource = await Repository.Provider.GetResourceAsync<PackageSearchResourceEnhancedV3>();
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

        private static FrameworkSpecificGroup GetMostCompatibleGroup(NuGetFramework projectTargetFramework, IEnumerable<FrameworkSpecificGroup> itemGroups)
        {
            var reducer = new FrameworkReducer();
            var mostCompatibleFramework = reducer.GetNearest(projectTargetFramework, itemGroups.Select(i => i.TargetFramework));

            if (mostCompatibleFramework != null)
            {
                var mostCompatibleGroup = itemGroups.FirstOrDefault(i => i.TargetFramework.Equals(mostCompatibleFramework));

                if (IsValid(mostCompatibleGroup))
                {
                    return mostCompatibleGroup;
                }
            }

            return null;
        }

        private static bool IsValid(FrameworkSpecificGroup frameworkSpecificGroup)
        {
            if (frameworkSpecificGroup != null)
            {
                return (frameworkSpecificGroup.HasEmptyFolder
                        || frameworkSpecificGroup.Items.Any()
                        || !frameworkSpecificGroup.TargetFramework.Equals(NuGetFramework.AnyFramework));
            }

            return false;
        }
    }

    public class PackageReference
    {
        public string AssemblyPath { get; set; }
        public string DocumentationPath { get; set; }
    }
}
