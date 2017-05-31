using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Subroute.Core.Compiler;
using Subroute.Api.Models.Compile;

namespace Subroute.Api.Controllers
{
    /// <summary>
    /// Provides stand-alone build functionality for arbitrary code.
    /// </summary>
    public class CompileController : ApiController
    {
        private readonly ICompilationService _compilationService;

        /// <summary>
        /// Constructs an instance of CompileController.
        /// </summary>
        /// <param name="compilationService">Compilation service to provided compiler functionality.</param>
        public CompileController(ICompilationService compilationService)
        {
            _compilationService = compilationService;
        }

        /// <summary>
        /// Compiles code from request and returns errors on build failure.
        /// </summary>
        /// <returns>Returns 204 No Content or build errors.</returns>
        [Route("compile/v1"), AllowAnonymous]
        public async Task<IHttpActionResult> PostCompileAsync(CompileRequest request)
        {
            var source = new Source(request.Code, request.Dependencies);
            var compilationResult = _compilationService.Compile(source);

            // Return an error response if compile was unsuccessful, otherwise the response was successful.
            return Content(compilationResult.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError, compilationResult);
        }
    }
}
