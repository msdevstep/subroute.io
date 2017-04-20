using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using Subroute.Api.Models.RouteSettings;
using Subroute.Core.Data;
using Subroute.Core.Data.Repositories;
using Subroute.Core.Exceptions;
using Subroute.Core.Extensions;
using Subroute.Core.Models.Routes;

namespace Subroute.Api.Controllers
{
    public class RouteSettingController : ApiController
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IRouteSettingRepository _routeSettingRepository;

        public RouteSettingController(IRouteRepository routeRepository, IRouteSettingRepository routeSettingRepository)
        {
            _routeRepository = routeRepository;
            _routeSettingRepository = routeSettingRepository;
        }

        [Route("routes/v1/{identifier}/settings")]
        public async Task<RouteSettingResponse[]> GetRouteSettingsAsync(RouteIdentifier identifier)
        {
            // Ensure that the current user is authorized to access this route.
            await EnsureAuthorizedRouteAccessAsync(identifier);

            // Load all route settings for the specified route identifier.
            var settings = await _routeSettingRepository.GetRouteSettingsAsync(identifier);

            // Map the RouteSetting types to RouteSettingResponse types.
            return settings.Select(RouteSettingResponse.Map).ToArray();
        }

        [Route("routes/v1/{identifier}/settings")]
        public async Task<RouteSettingResponse[]> PutRouteSettingsAsync(RouteIdentifier identifier, RouteSettingRequest[] settings)
        {
            // Ensure that the current user is authorized to access this route.
            var route = await EnsureAuthorizedRouteAccessAsync(identifier);

            // Map all of the incoming route settings to the internal DB type.
            // Ignore any blank settings with no name or value.
            var mappedSettings = settings
                .Where(s => s.Name.HasValue() && s.Value.HasValue())
                .Select(s => RouteSettingRequest.Map(s, route.Id))
                .ToArray();

            // All the route setting values will be specified, anything missing will be deleted,
            // anything new will be added, and any values that are different will be updated.
            // To compare, we'll need to load all the existing route settings.
            var existingSettings = await _routeSettingRepository.GetRouteSettingsAsync(identifier);
            
            // Find settings we need to add.
            var newSettings = mappedSettings.Where(s => existingSettings.All(es => !es.Name.CaseInsensitiveEqual(s.Name))).ToArray();
            var deleteSettings = existingSettings.Where(es => mappedSettings.All(s => !s.Name.CaseInsensitiveEqual(es.Name))).ToArray();
            var updateSettings = mappedSettings.Where(s => existingSettings.Any(es => es.Name.CaseInsensitiveEqual(s.Name) && !es.Value.CaseInsensitiveEqual(s.Value))).ToArray();
            var unchangedSettings = existingSettings.Where(es => settings.Any(s => s.Name.CaseInsensitiveEqual(es.Name) && s.Value.CaseInsensitiveEqual(es.Value))).ToArray();

            // Instantiate a new list to contain all the inserted and updated 
            // settings, this way we'll return any server generated ids.
            var results = new List<RouteSetting>();

            // Add all the unchanged settings to the results.
            results.AddRange(unchangedSettings);

            // We will execute all operations in a transaction so we don't end 
            // up with the settings in a mis-aligned state with the UI.
            using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Create new incoming settings.
                foreach (var setting in newSettings)
                    results.Add(await _routeSettingRepository.CreateRouteSettingAsync(setting));

                // Update settings where the value has changed.
                foreach (var setting in updateSettings)
                    results.Add(await _routeSettingRepository.UpdateRouteSettingAsync(setting));

                // Delete any missing settings.
                foreach (var setting in deleteSettings)
                    await _routeSettingRepository.DeleteRouteSettingAsync(setting);

                // Complete the transaction scope to commit all operations.
                ts.Complete();
            }

            // Map the RouteSetting type to the outgoing RouteSettingResponse type.
            return results.Select(RouteSettingResponse.Map).OrderBy(s => s.Name).ToArray();
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