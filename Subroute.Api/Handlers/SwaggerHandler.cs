using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Subroute.Api.Handlers
{
    public class SwaggerHandler : HttpMessageHandler
    {
        /// <summary>
        /// HTTP Handler to redirect the base url of adesa.auction.api to the Swager index.html page
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = request.CreateResponse(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri("swagger/ui/index", UriKind.Relative);
            return Task.FromResult(response);
        }
    }
}