using System;
using System.Threading.Tasks;
using System.Web.Http;
using Subroute.Api.Models.Requests;
using Subroute.Core.Data;
using Subroute.Core.Data.Repositories;
using Subroute.Core.Exceptions;
using Subroute.Core.Models.Routes;
using Request = Subroute.Core.Models.Requests.Request;

namespace Subroute.Api.Controllers
{
    /// <summary>
    /// Provides access to requests for a specified route.
    /// </summary>
    public class RequestController : ApiController
    {
        private readonly IRequestRepository _requestRepository;

        public RequestController(IRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        /// <summary>
        /// Loads request data for the specified route and date range.
        /// </summary>
        /// <param name="identifier">Integer ID or Uri of route.</param>
        /// <param name="take">Total number of records to return.</param>
        /// <param name="from">Start date of request data to load.</param>
        /// <param name="to">End date of request data to load.</param>
        /// <param name="skip">Starting index off records to return.</param>
        /// <returns>Returns an array of requests.</returns>
        [Route("routes/v1/{identifier}/requests")]
        public async Task<PagedCollection<Request>> GetRequestsByDateRangeAsync(RouteIdentifier identifier, int skip = 0, int take = 10, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            return await _requestRepository.GetRequestsByDateRangeAsync(identifier, skip, take, from, to);
        }

        /// <summary>
        /// Loads the total number of executions for the specified route.
        /// </summary>
        /// <param name="identifier">Integer ID or Uri of route.</param>
        /// <returns>Returns a long integer of the total number of executions.</returns>
        [Route("routes/v1/{identifier}/requests/count")]
        public async Task<long> GetRequestExecutionCountByRouteIdentifierAsync(RouteIdentifier identifier)
        {
            return await _requestRepository.GetRequestExecutionCountByRouteIdentifierAsync(identifier);
        }

        /// <summary>
        /// Returns the complete request record including the request and response payload.
        /// </summary>
        /// <param name="identifier">Unique identifier of the route.</param>
        /// <param name="id">Unique identifier of the request.</param>
        /// <returns>Returns the complete request record including the request and response payload.</returns>
        [Route("routes/v1/{identifier}/requests/{id}")]
        public async Task<RequestResponse> GetRequestByRequestId(RouteIdentifier identifier, int id)
        {
            var request = await _requestRepository.GetRequestByIdAsync(id);

            // Ensure the request belongs to the specified route.
            if (identifier.Type == RouteIdentifierType.Id && request.RouteId != identifier.Id)
                throw new NotFoundException("Request does not belong to the specified route.");
            
            if (identifier.Type == RouteIdentifierType.Uri && request.Route.Uri != identifier.Uri)
                throw new NotFoundException("Request does not belong to the specified route.");

            return RequestResponse.Map(request);
        }
    }
}