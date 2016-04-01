using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Subroute.Core.Exceptions;
using Subroute.Core.Extensions;
using Subroute.Core.Models.Routes;

namespace Subroute.Core.Data.Repositories
{
    public interface IRouteRepository
    {
        Route[] GetAllRoutes(string userId = null);
        Task<Route> GetRandomDefaultRouteAsync();
        PagedCollection<Route> GetAllPublicRoutes(int skip, int take, string search);
        Task<Route> GetRouteByIdentifierAsync(RouteIdentifier identifier, string userId = null);
        Task<Route> CreateRouteAsync(Route route, string userId = null);
        Task<Route> UpdateRouteAsync(Route route, string userId = null);
        Task DeleteRouteByIdentifierAsync(RouteIdentifier identifier, string userId = null);
    }

    public class RouteRepository : IRouteRepository
    {
        public Route[] GetAllRoutes(string userId = null)
        {
            // We'll load the route from an untracked collection since we don't want outside changes causing unwanted updates.
            using (var db = new SubrouteContext())
            {
                var query = db.Routes.Include(r => r.RouteSettings).AsNoTracking().AsQueryable();

                if (userId != null)
                    query = query.Where(q => q.UserId == userId);

                return query.ToArray();
            }
        }

        public async Task<Route> GetRandomDefaultRouteAsync()
        {
            using (var db = new SubrouteContext())
                return await db.Routes
                    .AsNoTracking()
                    .Where(r => r.IsDefault)
                    .OrderBy(r => Guid.NewGuid())
                    .FirstOrDefaultAsync();
        }

        public async Task<Route> GetRouteByIdentifierAsync(RouteIdentifier identifier, string userId = null)
        {
            using (var db = new SubrouteContext())
            {
                var query = db.Routes.Include(r => r.RouteSettings).AsNoTracking().AsQueryable();

                if (userId != null)
                    query = query.Where(q => q.UserId == userId);

                // We'll load the route from an untracked collection since we don't want outside changes causing unwanted updates.
                var route = identifier.Type == RouteIdentifierType.Id ?
                    await query.AsNoTracking().FirstOrDefaultAsync(r => r.Id == identifier.Id) :
                    await query.AsNoTracking().FirstOrDefaultAsync(r => r.Uri == identifier.Uri);

                if (route == null)
                    throw new NotFoundException($"No route with identifier '{identifier}' was found.");

                return route;
            }
        }

        public async Task<Route> CreateRouteAsync(Route route, string userId = null)
        {
            using (var db = new SubrouteContext())
            {
                // Ensure we are only creating a route for specified user.
                if (route.UserId != userId)
                    throw new NotFoundException("Route does not belong to specified user, and cannot be created.");

                // Set auto property values.
                route.CreatedOn = DateTimeOffset.Now;
                route.CreatedBy = "SYSTEM"; // Todo: Use Authenticated Username.
                route.UpdatedOn = route.CreatedOn;
                route.UpdatedBy = "SYSTEM"; // Todo: Use Authenticated Username.

                db.Routes.Add(route);
                await db.SaveChangesAsync();

                // Detach entry so its changes won't be tracked.
                db.Entry(route).State = EntityState.Detached;

                return route;
            }
        }

        public async Task<Route> UpdateRouteAsync(Route route, string userId = null)
        {
            using (var db = new SubrouteContext())
            {
                // Do not allow adding a new entry. Entry must already exist.
                if (route.Id == 0)
                    throw new NotFoundException("Route cannot be updated because it hasn't yet been created.");
                
                // Ensure route belongs to user.
                if (userId != null && route.UserId != userId)
                    throw new NotFoundException("Route does not belong to specified user, and cannot be modified.");
                
                db.Routes.Attach(route);
                db.Entry(route).State = EntityState.Modified;
                
                route.UpdatedOn = DateTimeOffset.Now;
                route.UpdatedBy = Thread.CurrentPrincipal.GetUserId();

                await db.SaveChangesAsync();

                // Detach entry so its changes won't be tracked.
                db.Entry(route).State = EntityState.Detached;

                return route;
            }
        }

        public async Task DeleteRouteByIdentifierAsync(RouteIdentifier identifier, string userId = null)
        {
            using (var db = new SubrouteContext())
            {
                var route = await GetRouteByIdentifierAsync(identifier);

                // Ensure route belongs to specified user.
                if (userId != null && route.UserId != userId)
                    throw new NotFoundException("Route does not belong to specified user, and cannot be deleted.");

                // Mark route for deletion so the context knowns to delete it.
                db.Entry(route).State = EntityState.Deleted;

                await db.SaveChangesAsync();
            }
        }

        public PagedCollection<Route> GetAllPublicRoutes(int skip, int take, string search)
        {
            using (var db = new SubrouteContext())
            {
                var query = db.Routes.Include(r => r.RouteSettings).AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query
                        .Where(q => !q.IsPrivate && q.Title.Contains(search));
                }
                else
                {
                    query = query
                        .Where(q => !q.IsPrivate && q.Title != null);
                }

                var count = query.Count();

                query = query.Take(take)
                    .Skip(skip)
                    .OrderBy(x => x.StarredCount)
                    .ThenBy(x => x.ClonedCount)
                    .ThenBy(x => x.UpdatedOn);

                return new PagedCollection<Route>() { TotalCount = count, Results = query.ToArray(), Skip = skip, Take = take };
            }
        }
    }
}
