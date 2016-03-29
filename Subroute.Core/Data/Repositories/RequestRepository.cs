using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Subroute.Core.Exceptions;
using Subroute.Core.Models.Routes;
using Subroute.Core.Utilities;

namespace Subroute.Core.Data.Repositories
{
    public interface IRequestRepository
    {
        Task<Request> GetRequestByIdAsync(int id);
        Task<Request> CreateRequestAsync(Request request);
        Task<Request> UpdateRequestAsync(Request request);
        Task<PagedCollection<Models.Requests.Request>> GetRequestsByDateRangeAsync(RouteIdentifier id, int skip, int take, DateTimeOffset? from, DateTimeOffset? to);
        Task<long> GetRequestExecutionCountByRouteIdentifierAsync(RouteIdentifier identifier);
    }

    public class RequestRepository : IRequestRepository
    {
        public async Task<Request> GetRequestByIdAsync(int id)
        {
            using (var db = new SubrouteContext())
            {
                var request = await db.Requests.Include(r => r.Route).AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

                if (request == null)
                    throw new NotFoundException($"No request with ID '{id}' was found.");

                return request;
            }
        }

        public async Task<PagedCollection<Models.Requests.Request>> GetRequestsByDateRangeAsync(RouteIdentifier id, int skip, int take, DateTimeOffset? from, DateTimeOffset? to)
        {
            using (var db = new SubrouteContext())
            {
                // Limit the request results by date range if one was provided.
                var query = db.Requests.AsNoTracking()
                    .Where(r => from.HasValue && r.OccurredOn >= from)
                    .Where(r => to.HasValue && r.OccurredOn <= to);

                // Ensure we are only returning requests for the specified route.
                query = id.Type == RouteIdentifierType.Id
                    ? query.Where(r => r.RouteId == id.Id)
                    : query.Where(r => r.Route.Uri == id.Uri);

                // Apply an order to the query to reverse chronological.
                query = query.OrderByDescending(r => r.OccurredOn);

                // Project and page query results into our wire format and materialize.
                // We project the results so we don't pull back the entire payload contents.
                var pagedResults = await query
                    .Skip(skip)
                    .Take(take)
                    .Select(Models.Requests.Request.Map)
                    .ToArrayAsync();

                // Get total count and final projected results.
                var total = await query.CountAsync();

                // Iterate the results and deserialize the headers for each request.
                foreach (var request in pagedResults)
                {
                    request.RequestHeaders = HeaderUtility.DeserializeHeaders(request.SerializedRequestHeaders);
                    request.ResponseHeaders = HeaderUtility.DeserializeHeaders(request.SerializedResponseHeaders);
                }

                return new PagedCollection<Models.Requests.Request>
                {
                    Skip = skip,
                    Take = take,
                    TotalCount = total,
                    Results = pagedResults
                };
            }
        }

        public async Task<long> GetRequestExecutionCountByRouteIdentifierAsync(RouteIdentifier identifier)
        {
            using (var db = new SubrouteContext())
            {
                // Build base query to return all requests.
                var query = db.Requests.AsNoTracking().AsQueryable();

                // Ensure we are only returning requests for the specified route.
                query = identifier.Type == RouteIdentifierType.Id
                    ? query.Where(r => r.RouteId == identifier.Id)
                    : query.Where(r => r.Route.Uri == identifier.Uri);

                // Execute query and get total count.
                return await query.LongCountAsync();
            }
        }

        public async Task<Request> CreateRequestAsync(Request request)
        {
            using (var db = new SubrouteContext())
            {
                request.OccurredOn = DateTimeOffset.UtcNow;

                // Calculate and store the request length if a request payload is available.
                if (request.RequestPayload != null)
                    request.RequestLength = request.RequestPayload.LongLength;

                db.Requests.Add(request);
                await db.SaveChangesAsync();

                // Detach entry so its changes won't be tracked.
                db.Entry(request).State = EntityState.Detached;

                return request;
            }
        }

        public async Task<Request> UpdateRequestAsync(Request request)
        {
            using (var db = new SubrouteContext())
            {
                // Do not allow adding a new entry. Entry must already exist.
                if (request.Id == 0)
                    throw new NotFoundException($"Request cannot be updated because it hasn't yet been created.");

                // Calculate and store the response length if a response payload is available.
                if (request.ResponsePayload != null)
                    request.ResponseLength = request.ResponsePayload.LongLength;

                db.Requests.Attach(request);
                db.Entry(request).State = EntityState.Modified;

                await db.SaveChangesAsync();

                // Detach entry so its changes won't be tracked.
                db.Entry(request).State = EntityState.Detached;

                return request;
            }
        }
    }
}