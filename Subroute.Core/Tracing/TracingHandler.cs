using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Subroute.Common;

namespace Subroute.Core.Tracing
{
    public class TracingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Execute the rest of the Web API pipeline so we can get the response object back.
            // We'll use the response object to attach the trace results to the response headers.
            var response = await base.SendAsync(request, cancellationToken);

            // We'll enable tracing if the request has the Enable-Tracing: true header.
            var traceContext = (ITraceContext)request.GetDependencyScope().GetService(typeof(ITraceContext));
            if (request.Headers.Contains("Enable-Tracing"))
                // Use the trace context to output the trace results, only if a trace context is present.
                traceContext?.OuputTracerResults(response);

            return response;
        }
    }
}