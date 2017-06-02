using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Subroute.Core.Nuget
{
    /// <summary>
    /// Provides instances of <see cref="PackageSearchResourceEnhancedV3"/> that perform NuGet package searches.
    /// </summary>
    public class PackageSearchResourceEnhancedV3Provider : ResourceProvider
    {
        /// <summary>
        /// Create an instance of <see cref="PackageSearchResourceEnhancedV3Provider"/>. The base constructor is
        /// also called that will associate <see cref="PackageSearchResourceEnhancedV3"/> with this provider.
        /// </summary>
        public PackageSearchResourceEnhancedV3Provider()
            : base(typeof(PackageSearchResourceEnhancedV3), "PackageSearchResourceEnhancedV3Provider", "PackageSearchResourceV2FeedProvider")
        {
            
        }

        /// <summary>
        /// Try to create an instance of <see cref="PackageSearchResourceEnhancedV3"/>.
        /// </summary>
        /// <param name="source">The repository that provides additional dependencies for the types that are created.</param>
        /// <param name="token">Cancellation token used to cancel creating the provider.</param>
        /// <returns>A tuple containing a boolean indicating whether an instance was able to be created and new instance of <see cref="PackageSearchResourceEnhancedV3"/>.</returns>
        public override async Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
        {
            // Ensure we can get an instance of the index resource as it provides the URIs for all the additional 
            // providers. NuGet uses this master index to combine all the services into one.
            if (await source.GetResourceAsync<ServiceIndexResourceV3>(token) == null)
                return new Tuple<bool, INuGetResource>(false, null);

            // The PackageSearchResourceEnhancedV3 instance uses the RawSearchResourceV3 to get the
            // raw JSON data so it can extract all the paging details and results. So get an instance
            // of this class and create the instance of PackageSearchResourceEnhancedV3 to return.
            var rawSearch = await source.GetResourceAsync<RawSearchResourceV3>(token);
            var curResource = new PackageSearchResourceEnhancedV3(rawSearch);

            // A tuple is used instead of an out parameter because out parameters aren't allowed for
            // async methods (yet). This is a limitation of C#.
            return new Tuple<bool, INuGetResource>(true, curResource);
        }
    }
}