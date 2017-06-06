using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Subroute.Core.Nuget
{
    /// <summary>
    /// Extension methods that instantiate all the providers that allow access to data in the NuGet gallery.
    /// </summary>
    public static class RepositoryFactoryExtensionsV3
    {
        /// <summary>
        /// Creates an instance of the requested <see cref="INuGetResource"/>.
        /// </summary>
        /// <typeparam name="TResource">Type of <see cref="INuGetResource"/> to be instantiated.</typeparam>
        /// <param name="factory">Instance of <see cref="Repository.ProviderFactory"/>.</param>
        /// <returns>Instance of <typeparam name="TResource"></typeparam>.</returns>
        public static async Task<TResource> GetResourceAsync<TResource>(this Repository.ProviderFactory factory)
            where TResource : class, INuGetResource
        {
            var providers = factory.GetPageableCoreV3();
            var packageSource = new PackageSource(Settings.NugetPackageUri);
            var sourceRepository = new SourceRepository(packageSource, providers);

            return await sourceRepository.GetResourceAsync<TResource>();
        }

        /// <summary>
        /// Provides standard V3 core provides except that it overrides the default <see cref="PackageSearchResourceV3Provider"/>
        /// with <see cref="PackageSearchResourceEnhancedV3Provider"/> that includes that top level result details such as
        /// the total number of results, last reopen date and time, and the name of the index used for the search.
        /// </summary>
        /// <param name="factory">Instance of <see cref="Repository.ProviderFactory"/>.</param>
        /// <returns>Instance of <see cref="IEnumerable{INuGetResourceProvider}"/> containing the provider instance.</returns>
        public static IEnumerable<INuGetResourceProvider> GetPageableCoreV3(this Repository.ProviderFactory factory)
        {
            // Return each of the standard core providers, but we are going to replace the standard PackageSearchResourceV3Provider
            // with our own custom implementation that will surface the statistics for each package search request. So return all
            // providers except that default PackageSearchResourceV3Provider because we'll return it seperately.
            foreach (var provider in Repository.Provider.GetCoreV3().Where(p => p.Value.ResourceType != typeof(PackageSearchResourceV3Provider)).Select(p => p.Value))
                yield return provider;

            yield return new PackageSearchResourceEnhancedV3Provider();
        }
    }
}