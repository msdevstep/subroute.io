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
using NuGet.Versioning;

namespace Subroute.Core.Nuget
{
    public class NugetService : INugetService
    {
        //private readonly IPackageRepository _PackageRepository = null;

        public static string[] TargetFrameworks = new[] {".NETFramework,Version=v4.5", ".NETFramework,Version=v4.0"};

        public NugetService()
        {
            //_PackageRepository = PackageRepositoryFactory.Default.CreateRepository(Settings.NugetPackageUri);
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

        public string DownloadPackage(NugetPackage package)
        {
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
            return null;
        }

        public NugetPackage[] ResolveDependencies(Dependency dependency)
        {
            return null;
            //// Find the actual nuget package from the gallery to get its dependencies.
            //var package = _PackageRepository.FindPackage(dependency.Id, SemanticVersion.Parse(dependency.Version));

            //if (package == null)
            //    throw new NotFoundException($"Unable to locate package. Package ID: {dependency.Id}, Version: {dependency.Version}.");

            //// Recursively locate any additional nuget dependencies for this package that their
            //// packages, and combine into a single output array.
            //return new[] { NugetPackage.Map(package) }
            //    .Concat(ResolveDependencies(package))
            //    .ToArray();
        }

        private async Task<NugetPackage[]> ResolveDependenciesAsync(IPackageSearchMetadata package)
        {
            // Get an instance of the provider used to get package result data from the NuGet gallery.
            var providers = Provider.GetPageableCoreV3().ToList();
            var packageSource = new PackageSource(Settings.NugetPackageUri);
            var sourceRepository = new SourceRepository(packageSource, providers);
            var packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
            var searchMetadata = await packageMetadataResource.GetMetadataAsync(package.Identity, new TraceLogger(), CancellationToken.None);

            // Get an array of NuGet only depdencies.
            var dependencies = searchMetadata
                .DependencySets
                .Where((ds, i) => ds.TargetFramework.IsAny && i == 0)
                .SelectMany(ds => ds.Packages)
                .ToArray();

            var results = new List<NugetPackage>();

            foreach (var dependency in dependencies)
            {
                var bestVersion = await GetHighestMatchingVersion(dependency.Id, dependency);
                var dependencyPackage = await packageMetadataResource.GetMetadataAsync(new PackageIdentity(dependency.Id, bestVersion), new TraceLogger(), CancellationToken.None);

                results.Add(NugetPackage.Map(dependencyPackage));

                var dependencyPackages = await ResolveDependenciesAsync(dependencyPackage);
                results.AddRange(dependencyPackages);
            }

            return results.ToArray();

            //dependencyInfo.Where(di => di.)

            return null;
            //var walker = new DependentsWalker(_PackageRepository, new FrameworkName(".NETFramework", Version.Parse("4.6")));
            //walker.DependencyVersion = DependencyVersion.Highest;
            //walker.SkipPackageTargetCheck = true;
            //var dependencies = walker.GetDependents(package);

            ////// Find nuget only dependencies.
            ////var dependencies = package.DependencySets
            ////    .Where(ds => ds.TargetFramework == null && !ds.SupportedFrameworks.Any())
            ////    .SelectMany(ds => ds.Dependencies)
            ////    .ToArray();

            ////if (!dependencies.Any())
            ////    return new NugetPackage[0];

            ////// Find actual packages in nuget gallery by IVersionSpec.
            ////var foundDependencies = dependencies
            ////    .Select(p => _PackageRepository.FindPackage(p.Id, p.VersionSpec, true, true))
            ////    .ToArray();

            ////// Determine which packages we could not locate.
            ////var missing = dependencies.Where(d => !foundDependencies.Any(fd => fd.Id == d.Id)).ToArray();

            ////if (missing.Any())
            ////    throw new NotFoundException($"Unable to locate dependent packages: {string.Join(", ", missing.Select(m => m.Id))}");

            //// Determine if any of these dependencies have other nuget dependencies.
            //return dependencies
            //    .Select(NugetPackage.Map)
            //    //.Concat(foundDependencies.SelectMany(fd => ResolveDependencies(fd)))
            //    .ToArray();
        }

        private async Task<NuGetVersion> GetHighestMatchingVersion(string packageId, PackageDependency dependency)
        {
            var providers = Provider.GetPageableCoreV3().ToList();
            var packageSource = new PackageSource(Settings.NugetPackageUri);
            var sourceRepository = new SourceRepository(packageSource, providers);
            var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();
            var dependencyInfo = await dependencyInfoResource.ResolvePackages(packageId, GetCurrentFramework(), new TraceLogger(), CancellationToken.None);

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
            var providers = Provider.GetPageableCoreV3().ToList();
            var packageSource = new PackageSource(Settings.NugetPackageUri);
            var sourceRepository = new SourceRepository(packageSource, providers);
            var searchResource = await sourceRepository.GetResourceAsync<PackageSearchResourceEnhancedV3>();
            var searchFilter = new SearchFilter(false, SearchFilterType.IsLatestVersion);
            var searchMetadata = await searchResource.SearchPageableAsync(term,
                searchFilter,
                skip.GetValueOrDefault(),
                take.GetValueOrDefault(),
                new TraceLogger(),
                CancellationToken.None);

            var package = searchMetadata.Results.FirstOrDefault();

            if (package != null)
                await ResolveDependenciesAsync(package);

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

    public class Project : NuGetProject
    {
        public override Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
