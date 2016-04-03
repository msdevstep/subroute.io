using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Subroute.Core.Tracing;

namespace Subroute.Api.Handlers
{
    public class SecureHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                var response = request.CreateResponse(HttpStatusCode.Found);
                var sslUri = new UriBuilder(request.RequestUri)
                {
                    Scheme = Uri.UriSchemeHttps,
                    Port = 443
                };

                response.Headers.Location = sslUri.Uri;
                response.ReasonPhrase = "SSL is required to access API.";

                return response;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}