using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using Subroute.Api.Models.RoutePackages;
using Subroute.Core.Data;
using Subroute.Core.Data.Repositories;
using Subroute.Core.Exceptions;
using Subroute.Core.Extensions;
using Subroute.Core.Models.Routes;
using Subroute.Core.Nuget;
using NuGet;
using Subroute.Core.Compiler;
using Subroute.Core.Models.Compiler;

namespace Subroute.Api.Controllers
{
    public class RoutePackageController : ApiController
    {
        private readonly INugetService _nugetService;
        private readonly IRouteRepository _routeRepository;
        private readonly IRoutePackageRepository _routePackageRepository;
        private readonly IMetadataProvider _metadataProvider;

        public RoutePackageController(IRouteRepository routeRepository, IRoutePackageRepository routePackageRepository, INugetService nugetService, IMetadataProvider metadataProvider)
        {
            _nugetService = nugetService;
            _routeRepository = routeRepository;
            _routePackageRepository = routePackageRepository;
            _metadataProvider = metadataProvider;
        }

        [Route("routes/v1/{identifier}/packages")]
        public async Task<RoutePackageResponse[]> GetRoutePackagesAsync(RouteIdentifier identifier)
        {
            // Ensure that the current user is authorized to access this route.
            await EnsureAuthorizedRouteAccessAsync(identifier);

            // Load all route packages for the specified route identifier.
            var packages = await _routePackageRepository.GetRoutePackagesAsync(identifier, true);

            // Map the RoutePackage types to RoutePackageResponse types.
            return packages.Select(RoutePackageResponse.Map).ToArray();
        }

        [Route("routes/v1/{identifier}/packages")]
        public async Task<RoutePackageResponse[]> PutRoutePackagesAsync(RouteIdentifier identifier, RoutePackageRequest[] packages)
        {
            // Ensure that the current user is authorized to access this route.
            var route = await EnsureAuthorizedRouteAccessAsync(identifier);

            // Ensure all provided packages are valid.
            if (packages.Any(p => !p.Id.HasValue() || !p.Version.HasValue()))
                throw new BadRequestException($"Please ensure each route package has a valid 'id' and 'version'.");

            // Expand all package dependencies so we have every dependency that the code needs to properly compile.
            // Convert input packages to a Dependency type so we can resolve its dependencies. Take all resolved
            // dependency packages and convert them to our database RoutePackage type to be stored. We'll also
            // indicate which packages were user specified, so that we only show the user chosen packages.
            var dependencies = packages.Select(p => p.ToDependency());
            var resolved = await _nugetService.ResolveAllDependenciesAsync(dependencies);
            var mappedPackages = resolved.Select(r => r.ToRoutePackage(route.Id, packages.Any(p => p.Id == r.Id))).ToArray();

            // All the route package values will be specified, anything missing will be deleted,
            // anything new will be added, and any values that are different will be updated.
            // To compare, we'll need to load all the existing route packages.
            var existingPackages = await _routePackageRepository.GetRoutePackagesAsync(identifier);
            
            // Find packages we need to add.
            var newPackages = mappedPackages.Where(s => existingPackages.All(es => !es.Id.CaseInsensitiveEqual(s.Id))).ToArray();
            var deletePackages = existingPackages.Where(es => mappedPackages.All(s => !s.Id.CaseInsensitiveEqual(es.Id))).ToArray();
            var updatePackages = mappedPackages.Where(s => existingPackages.Any(es => es.Id.CaseInsensitiveEqual(s.Id) && (!es.Version.CaseInsensitiveEqual(s.Version) || es.UserSpecified != s.UserSpecified))).ToArray();
            
            // We will execute all operations in a transaction so we don't end 
            // up with the packages in a mis-aligned state with the UI.
            using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Create new incoming packages.
                foreach (var package in newPackages)
                    await _routePackageRepository.CreateRoutePackageAsync(package);

                // Update packages where the value has changed.
                foreach (var package in updatePackages)
                    await _routePackageRepository.UpdateRoutePackageAsync(package);

                // Delete any missing packages.
                foreach (var package in deletePackages)
                    await _routePackageRepository.DeleteRoutePackageAsync(package);

                // Complete the transaction scope to commit all operations.
                ts.Complete();
            }

            // Pull a fresh list of the route packages and only show user specified packages.
            var results = await _routePackageRepository.GetRoutePackagesAsync(identifier, true);

            // Download all packages and their dependencies so they are ready to go for
            // intellisense or compilation.
            foreach (var package in results)
                await _nugetService.DownloadPackageAsync(package);

            return results.Select(RoutePackageResponse.Map).OrderBy(s => s.Id).ToArray();
        }

        private async Task<Route> EnsureAuthorizedRouteAccessAsync(RouteIdentifier identifier)
        {
            // Load the route by identifier, this method will throw an exception if no route exists.
            var route = await _routeRepository.GetRouteByIdentifierAsync(identifier);

            // We've found a route, we need to ensure the user owns the route or they are an admin.
            if (!route.UserId.CaseInsensitiveEqual(User.GetUserId()) && !User.IsAdmin())
                throw new AuthorizationException("The current user is not authorized to access specified route.");

            return route;
        }
    }
}