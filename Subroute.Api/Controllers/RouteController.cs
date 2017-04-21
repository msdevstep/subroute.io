using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using Auth0.Core.Http;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Clients;
using Subroute.Api.Models.Routes;
using Subroute.Core;
using Subroute.Core.Data;
using Subroute.Core.Data.Repositories;
using Subroute.Core.Exceptions;
using Subroute.Core.Extensions;
using Subroute.Core.Models.Routes;

namespace Subroute.Api.Controllers
{
    /// <summary>
    /// Provides functionality to create, retrieve, update, and delete routes.
    /// </summary>
    public class RouteController : ApiController
    {
        private readonly IRouteRepository _routeRepository;

        /// <summary>
        /// Constructs a new instance of RouteController.
        /// </summary>
        /// <param name="routeRepository">Dynamically injected RouteRepository for working with routes.</param>
        public RouteController(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        /// <summary>
        /// Load all routes for the authenticated user.
        /// </summary>
        /// <returns>Returns an array of routes.</returns>
        [Route("routes/v1/")]
        public RouteResponse[] GetAllRoutes()
        {
            // Extract user ID from token to use to filter routes.
            var userId = User.GetUserId();
             
            // We may not have a user ID due to impersonation. Throw an exception.
            if (userId == null)
                throw new AuthenticationException("Unable to determine user ID of current user.");

            // We'll use our map function to map our internal Route objects to our RouteResponse wire format.
            return _routeRepository.GetAllRoutes(userId).Select(RouteResponse.Map).ToArray();
        }

        /// <summary>
        /// Loads a single route by its identifier (either ID or Uri).
        /// </summary>
        /// <param name="identifier">Integer ID or Uri of route.</param>
        /// <returns>Returns a single route.</returns>
        [Route("routes/v1/{identifier}")]
        public async Task<RouteResponse> GetRouteByIdentiferAsync(RouteIdentifier identifier)
        {
            var userId = User.GetUserId();
            var route = await _routeRepository.GetRouteByIdentifierAsync(identifier, userId);
            return RouteResponse.Map(route);
        }

        [Route("routes/v1/default/random"), AllowAnonymous]
        public async Task<DefaultResponse> GetRandomDefaultRouteAsync()
        {
            var route = await _routeRepository.GetRandomDefaultRouteAsync();

            if (route == null)
                throw new NotFoundException("No saved default routes exists.");

            var client = new ManagementApiClient(Settings.SubrouteAuth0ManagementApiToken, new Uri(Settings.SubrouteAuth0ManagementApiUri));
            var user = await client.Users.GetAsync(route.UserId);
            
            return new DefaultResponse
            {
                Id = route.Id,
                UserId = route.UserId,
                Name = user?.FullName,
                Picture = user?.Picture,
                Title = route.Title,
                Code = route.Code
            };
        }

        /// <summary>
        /// Creates a single route.
        /// </summary>
        /// <param name="routeRequest">Object containing the route data.</param>
        /// <returns>Returns a single route.</returns>
        [Route("routes/v1/")]
        public async Task<RouteResponse> PostCreateRouteAsync(RouteRequest routeRequest)
        {
            // We'll use the patch method to set the properties values from the request to a new internal Route class.
            var userId = User.GetUserId();
            var route = new Route();
            routeRequest.Patch(route);

            route = await _routeRepository.CreateRouteAsync(route, userId);

            return RouteResponse.Map(route);
        }

        /// <summary>
        /// Updates the specified route using only the provided properties.
        /// </summary>
        /// <param name="identifier">Integer ID or Uri of route.</param>
        /// <param name="values">Object containing just the properties to be updated.</param>
        /// <returns>Returns a single route.</returns>
        [Route("routes/v1/{identifier}")]
        public async Task<RouteResponse> PatchRouteByIdentiferAsync(RouteIdentifier identifier, IDictionary<string, object> values)
        {
            // We'll get the intersection of known valid property names, to the incoming property
            // names, as this will get us a list of only valid incoming property keys.
            var userId = User.GetUserId();
            var route = await _routeRepository.GetRouteByIdentifierAsync(identifier, userId);
            var validProperties = new List<string> { "owner", "uri", "title", "type", "code", "isOnline" };

            // Allow the IsDefault property to be set if the user is an admin.
            if (User.IsAdmin())
                validProperties.Add("isDefault");

            var selectedProperties = values.Keys.Intersect(validProperties);

            // Only set allowed properties using reflection and type coersion. We proper-case the
            // name because our serializer automatically camel-cases json property names.
            foreach (var property in selectedProperties)
                route.SetProperty(property.ToProperCase(), values[property]);

            // Update server properties.
            route.IsCurrent = false;
            route = await _routeRepository.UpdateRouteAsync(route, userId);

            return RouteResponse.Map(route);
        }

        /// <summary>
        /// Deletes a single route by its identifier (either ID or Uri).
        /// </summary>
        /// <param name="identifier">Integer ID or Uri of route.</param>
        /// <returns>Returns 204 No Content.</returns>
        [Route("routes/v1/{identifier}")]
        public async Task DeleteRouteByIdentiferAsync(RouteIdentifier identifier)
        {
            var userId = User.GetUserId();
            await _routeRepository.DeleteRouteByIdentifierAsync(identifier, userId);
        }
    }
}