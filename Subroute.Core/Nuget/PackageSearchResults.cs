using System;
using NuGet.Protocol.Core.Types;

namespace Subroute.Core.Nuget
{
    /// <summary>
    /// Contains top-level package search result data useful for server-side paging.
    /// </summary>
    public class PackageSearchResults
    {
        /// <summary>
        /// Gets or sets to total number of records that match the search parameters.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the last reopen.
        /// </summary>
        public DateTimeOffset LastReopen { get; set; }

        /// <summary>
        /// Gets or sets the name of the index used for the search in the NuGet database.
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Gets or sets an array of package search results.
        /// </summary>
        public IPackageSearchMetadata[] Results { get; set; }
    }
}