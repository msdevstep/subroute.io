using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Subroute.Core.Data;
using Subroute.Core.Data.Repositories;
using Subroute.Core.Exceptions;
using Subroute.Core.Models.Routes;
using Subroute.Core.ServiceBus;
using Subroute.Core.Tracing;
using Subroute.Core.Utilities;

namespace Subroute.Core.Execution
{
    public interface IExecutionService
    {
        Task<ExecutionResponse> ExecuteRouteAsync(string route, ExecutionRequest request);
    }

    public class ExecutionService : IExecutionService
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly ITopicFactory _topicFactory;
        private readonly IResponsePipeline _responsePipeline;
        private readonly ITraceContext _traceContext;

        public ExecutionService(IRouteRepository routeRepository, IRequestRepository requestRepository, ITopicFactory topicFactory, IResponsePipeline responsePipeline, ITraceContext traceContext)
        {
            _routeRepository = routeRepository;
            _requestRepository = requestRepository;
            _topicFactory = topicFactory;
            _responsePipeline = responsePipeline;
            _traceContext = traceContext;
        }
        
        public async Task<ExecutionResponse> ExecuteRouteAsync(string route, ExecutionRequest request)
        {
            // Load the route to be executed so we can get its identifier.
            var routeEntry = await _traceContext.TraceTimeAsync("Load-Route", _routeRepository.GetRouteByIdentifierAsync(route));

            // Ensure the route is online.
            if (!routeEntry.IsOnline)
                throw new OfflineException("Route is offline.");

            // Ensure the route is published.
            if (!routeEntry.PublishedOn.HasValue)
                throw new OfflineException("Route has not yet been published.");

            // Create request data in database which we'll match the response to after execution.
            var serializedHeaders = HeaderUtility.SerializeHeaders(request.Headers);
            var requestEntry = new Request
            {
                RouteId = routeEntry.Id, 
                Method = request.Method.ToString(),
                Uri = request.Uri.ToString(),
                RequestHeaders = serializedHeaders,
                RequestPayload = request.Body,
                IpAddress = request.IpAddress,
                OccurredOn = DateTimeOffset.UtcNow
            };
            requestEntry = await _traceContext.TraceTimeAsync("Create-Request", _requestRepository.CreateRequestAsync(requestEntry));

            // We will send just a pointer to the request in the database via the service bus to side step size restrictions.
            // We'll use the machine name as the correlation ID to allow the message to be returned to this waiting instance.
            var requestMessage = new BrokeredMessage
            {
                ReplyTo = Settings.ServiceBusResponseTopicName,
                CorrelationId = Environment.MachineName
            };
            requestMessage.Properties["RouteId"] = routeEntry.Id;
            requestMessage.Properties["RequestId"] = requestEntry.Id;
            requestMessage.Properties["MachineName"] = Environment.MachineName;

            var topicClient = await _traceContext.TraceTimeAsync("Create-Topic-Client", _topicFactory.CreateTopicClientAsync(Settings.ServiceBusRequestTopicName));
            await _traceContext.TraceTimeAsync("Send-Request", topicClient.SendAsync(requestMessage));
            
            // We'll try to get a message every second to find our desired message. Otherwise we'll keep trying for up to configured duration.
            var maxWaitTime = TimeSpan.FromMinutes(Settings.ServiceBusResponseTimeoutMinutes);
            
            try
            {
                // We'll listen on the response pipeline observable for incoming messages that originated as
                // requests from this machine, and are filtered by CorrelationId (which is the machine name).
                // We have added a filter that will only listen for messages generated from this waiting
                // thread. We'll also specify that we'll wait no longer than the maximum specified timeout.
                // When we receive the message, we'll load the details from the database and convert it to
                // an ExecutionResponse object and select the first one (which there should only be one).
                var executionResponse = await _traceContext.TraceTimeAsync("Receive-Generate-Response", _responsePipeline.ResponseMessages
                    .Where(bm => (int) bm.Properties["RequestId"] == requestEntry.Id)
                    .Timeout(maxWaitTime)
                    .Select(async bm =>
                    {
                        // We have found our message, requery the database to get the response data.
                        var responseEntry = await _traceContext.TraceTimeAsync("Load-Request-By-ID", _requestRepository.GetRequestByIdAsync(requestEntry.Id));

                        // Build a ExecutionResponse using details from database.
                        var responseStatusCode = (HttpStatusCode) (responseEntry.StatusCode ?? 204);
                        var deserializedHeaders = _traceContext.TraceTime("Deserialize-Headers", () => ExecutionResponse.DeserializeHeaders(responseEntry.ResponseHeaders));
                        var response = new ExecutionResponse(responseStatusCode)
                        {
                            StatusMessage = responseEntry.StatusMessage,
                            Body = responseEntry.ResponsePayload,
                            Headers = deserializedHeaders
                        };

                        return response;
                    }).FirstOrDefault());

                // We'll only arrive here if we actually received a matching message. Otherwise
                // a TimeoutException will be thrown and will produce a timeout response.
                return executionResponse;
            }
            catch (TimeoutException)
            {
                // Maximum wait time elapsed, create an empty response and return it.
                return _traceContext.TraceTime("Create-Empty-Response", () => CreateTimeoutResponse(route, requestEntry.Id));
            }
        }

        private ExecutionResponse CreateTimeoutResponse(string route, int requestId)
        {
            var response = new ExecutionResponse(HttpStatusCode.SeeOther);
            var responseUri = $"{Settings.ApiBaseUri}/v1/{route}/{requestId}";

            response.Headers.Add("Location", responseUri);
            response.StatusMessage = "Execution Timeout - Follow Location Header for Response";

            return response;
        }
    }
}
