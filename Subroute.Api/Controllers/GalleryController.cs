using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Subroute.Api.Models.Routes;
using Subroute.Core.Data;
using Subroute.Core.Data.Repositories;
using Subroute.Core.Extensions;

namespace Subroute.Api.Controllers
{
    public class GalleryController : ApiController
    {
        private readonly IRouteRepository _routeRepository;

        public GalleryController(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        /// <summary>
        /// Load all public routes.
        /// </summary>
        /// <returns>Returns an paged collection containing an array of routes.</returns>
        [AllowAnonymous]
        [Route("gallery/v1/")]
        public PagedCollection<RouteResponse> GetAllPublicRoutes(int skip, int take, string search)
        {
            var result = _routeRepository.GetAllPublicRoutes(skip, take, search);
            var entities = result.Results.Select(RouteResponse.Map).ToArray();

            return new PagedCollection<RouteResponse>() { Results = entities, TotalCount = result.TotalCount, Skip = result.Skip, Take = result.Take };
        }
    }
}
