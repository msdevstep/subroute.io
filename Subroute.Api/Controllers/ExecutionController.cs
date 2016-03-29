using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Subroute.Api.Models.HttpActionResults;
using Subroute.Core.Execution;
using Subroute.Core.Extensions;

namespace Subroute.Api.Controllers
{
    public class ExecutionController : ApiController
    {
        private readonly IExecutionService _executionService;

        public ExecutionController(IExecutionService executionService)
        {
            _executionService = executionService;
        }

        [Route("v1/{route}"), AcceptVerbs("GET", "POST", "PUT", "PATCH", "MERGE", "DELETE", "OPTIONS", "HEAD"), AllowAnonymous]
        public async Task<IHttpActionResult> ExecuteRouteAsync(string route)
        {
            // Read entire request stream into memory so we can get the total length.
            await Request.Content.LoadIntoBufferAsync();

            // Get the entire stream in memory and build an ExecutionRequest to pass to execution engine.
            var requestBytes = await Request.Content.ReadAsByteArrayAsync();
            var request = new ExecutionRequest(Request.RequestUri, Request.Method)
            {
                Headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value),
                Body = requestBytes,
                IpAddress = Request.GetIpAddress()
            };

            // Call internal exeuction system to execute microservice, and wait for the response.
            var response = await _executionService.ExecuteRouteAsync(route, request);

            // Use our custom action result to translate internal response to HttpResponseMessage.
            return new ExecutionResponseActionResult(response);
        }
    }
}