using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Subroute.Core.Nuget
{
    /// <summary>
    /// Provides access to the package data from the NuGet gallery and includes top-level result details
    /// that make server-side paging possible.
    /// </summary>
    public class PackageSearchResourceEnhancedV3 : PackageSearchResource
    {
        private readonly RawSearchResourceV3 _searchResource;

        /// <summary>
        /// Constructs an instance of <see cref="PackageSearchResourceEnhancedV3"/>.
        /// </summary>
        /// <param name="searchResource">Instance of <see cref="RawSearchResourceV3"/> that provides direct access to the result JSON data.</param>
        public PackageSearchResourceEnhancedV3(RawSearchResourceV3 searchResource)
        {
            _searchResource = searchResource;
        }

        /// <summary>
        /// Searches the NuGet gallery for data matching the provided search term.
        /// </summary>
        /// <param name="searchTerm">Search term used to filter results from NuGet gallery.</param>
        /// <param name="filters">Additional details used to limit the search results.</param>
        /// <param name="skip">Record index of the starting record to be returned.</param>
        /// <param name="take">Number of result records to include in the search results.</param>
        /// <param name="log">Logger used to capture debugging information during the search.</param>
        /// <param name="cancellationToken">Token used to interrupt the search.</param>
        /// <returns>An instance of <see cref="IEnumerable{IPackageSearchMetadata}"/> containing search results only.</returns>
        public override async Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(string searchTerm, SearchFilter filters, int skip, int take, ILogger log, CancellationToken cancellationToken)
        {
            // Use our enhanced implementation to get results with statistics which we won't be using.
            var result = await SearchPageableAsync(searchTerm, filters, skip, take, log, cancellationToken);
            
            // The existing framework doesn't have a way to pass back result stats, so to satisfy their
            // contract, we'll just use our inner implementation and only return the metadata results.
            return result.Results;
        }

        /// <summary>
        /// Searches the NuGet gallery for data matching the provided search term.
        /// </summary>
        /// <param name="searchTerm">Search term used to filter results from NuGet gallery.</param>
        /// <param name="filters">Additional details used to limit the search results.</param>
        /// <param name="skip">Record index of the starting record to be returned.</param>
        /// <param name="take">Number of result records to include in the search results.</param>
        /// <param name="log">Logger used to capture debugging information during the search.</param>
        /// <param name="cancellationToken">Token used to interrupt the search.</param>
        /// <returns>An instance of <see cref="PackageSearchResults"/> containing paging data and search results.</returns>
        public async Task<PackageSearchResults> SearchPageableAsync(string searchTerm, SearchFilter filters, int skip, int take, ILogger log, CancellationToken cancellationToken)
        {
            var response = await _searchResource.SearchPage(searchTerm, filters, skip, take, NullLogger.Instance, cancellationToken);

            return new PackageSearchResults
            {
                TotalCount = (int) response["totalHits"],
                LastReopen = DateTimeOffset.Parse((string) response["lastReopen"]),
                Index = (string) response["index"],
                Results = response["data"]
                    .Values<JObject>()
                    .Select(t => JsonExtensions.FromJToken<PackageSearchMetadata>(t))
                    .Select(m => m.WithVersions(GetVersions(m, filters)))
                    .ToArray()
            };
        }

        /// <summary>
        /// Gets an array of version information for a specific package.
        /// </summary>
        /// <param name="metadata">Specific package used to obtain the version information.</param>
        /// <param name="filter">Additional details used to limit the search results.</param>
        /// <returns>An instance of <see cref="IEnumerable{VersionInfo}"/> containing version information.</returns>
        private static IEnumerable<VersionInfo> GetVersions(PackageSearchMetadata metadata, SearchFilter filter)
        {
            var parsedVersions = metadata.ParsedVersions;
            var totalDownloadCount = parsedVersions
                .Select(v => v.DownloadCount)
                .Sum();

            return parsedVersions
                .Select(v => v.Version)
                .Where(v => filter.IncludePrerelease || !v.IsPrerelease)
                .Concat(new [] { metadata.Version })
                .Distinct()
                .Select(v => new VersionInfo(v, totalDownloadCount))
                .ToArray();
        }
    }
}