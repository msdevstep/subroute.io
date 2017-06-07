using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Subroute.Api.Models.Publish;
using Subroute.Api.Models.Routes;
using Subroute.Core.Compiler;
using Subroute.Core.Data.Repositories;
using Subroute.Core.Models.Routes;
using Subroute.Core.Models.Compiler;
using System.Linq;

namespace Subroute.Api.Controllers
{
    /// <summary>
    /// Provides publish functionality for saved code.
    /// </summary>
    public class PublishController : ApiController
    {
        private readonly IRouteRepository _routeRepository;
        private readonly ICompilationService _compilationService;

        public PublishController(IRouteRepository routeRepository, ICompilationService compilationService)
        {
            _routeRepository = routeRepository;
            _compilationService = compilationService;
        }

        /// <summary>
        /// Compiles and publishes saved code or returns errors on build failure.
        /// </summary>
        /// <param name="identifier">Integer ID or Uri of route.</param>
        /// <returns>Returns 204 No Content or build errors.</returns>
        [Route("routes/v1/{identifier}/publish")]
        public async Task<IHttpActionResult> PostPublishAsync(RouteIdentifier identifier)
        {
            // Load route to get source code.
            var route = await _routeRepository.GetRouteByIdentifierAsync(identifier);
            var code = route.Code;
            var source = new Source(code, route.RoutePackages.Select(p => new Dependency
            {
                Id = p.Id,
                Version = p.Version,
                Type = p.Type
            }).ToArray());

            // Compile code and get assembly as byte array.
            var compilationResult = await _compilationService.CompileAsync(source);

            // We'll save the assembly and mark the route as published if the compilation was successful.
            if (compilationResult.Success)
            {
                route.Assembly = compilationResult.Assembly;
                route.IsCurrent = true;
                route.IsOnline = true;
                route.PublishedOn = DateTimeOffset.UtcNow;

                route = await _routeRepository.UpdateRouteAsync(route);
            }

            var result = new PublishResponse
            {
                Compilation = compilationResult,
                Route = RouteResponse.Map(route)
            };

            // Return an error response if compile was unsuccessful, otherwise the response was successful.
            return Content(compilationResult.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError, result);
        }
    }
}