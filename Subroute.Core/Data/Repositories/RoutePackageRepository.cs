using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Subroute.Core.Exceptions;
using Subroute.Core.Extensions;
using Subroute.Core.Models.Routes;

namespace Subroute.Core.Data.Repositories
{
    public interface IRoutePackageRepository
    {
        Task<RoutePackage[]> GetRoutePackagesAsync(RouteIdentifier identifier);
        Task<RoutePackage> GetRoutePackageByIdAsync(RouteIdentifier identifier, string id);
        Task<RoutePackage> CreateRoutePackageAsync(RoutePackage package);
        Task<RoutePackage> UpdateRoutePackageAsync(RoutePackage package);
        Task DeleteRoutePackageAsync(RouteIdentifier identifier, string id);
        Task DeleteRoutePackageAsync(RoutePackage package);
    }

    public class RoutePackageRepository : IRoutePackageRepository
    {
        /// <summary>
        /// Load all the route packages for the specified <see cref="RouteIdentifier"/>.
        /// </summary>
        /// <param name="identifier"><see cref="RouteIdentifier"/> for the requested route.</param>
        /// <returns>Returns an array of <see cref="RoutePackage"/> objects for the specified route.</returns>
        public async Task<RoutePackage[]> GetRoutePackagesAsync(RouteIdentifier identifier)
        {
            using (var db = new SubrouteContext())
            {
                // Get an untracked base queryable.
                var query = db.RoutePackages
                    .AsNoTracking()
                    .AsQueryable();

                // Ensure we are only returning packages for the specified route
                // Use the correct identifier type to query the database.
                query = identifier.Type == RouteIdentifierType.Id
                    ? query.Where(r => r.RouteId == identifier.Id)
                    : query.Where(r => r.Route.Uri == identifier.Uri);

                // Materialize the results into memory.
                return await query.ToArrayAsync();
            }
        }

        /// <summary>
        /// Load a single route package by its id.
        /// </summary>
        /// <param name="identifier"><see cref="RouteIdentifier"/> for the requested route.</param>
        /// <param name="id">String representing the specified route package id.</param>
        /// <returns>Returns a single <see cref="RoutePackage"/> with the matching route package identifier.</returns>
        public async Task<RoutePackage> GetRoutePackageByIdAsync(RouteIdentifier identifier, string id)
        {
            using (var db = new SubrouteContext())
            {
                var query = db.RoutePackages
                    .AsNoTracking()
                    .Where(rs => rs.Id == id);
                
                // Ensure we are only returning packages for the specified route
                // Use the correct identifier type to query the database.
                query = identifier.Type == RouteIdentifierType.Id
                    ? query.Where(r => r.RouteId == identifier.Id)
                    : query.Where(r => r.Route.Uri == identifier.Uri);

                // Return the first match.
                return await query.FirstOrDefaultAsync();
            }
        }

        /// <summary>
        /// Uses the specified <see cref="RoutePackage"/> instance to create a new route package record in the database.
        /// </summary>
        /// <param name="setting">Instance of <see cref="RoutePackage"/> used to create the database record.</param>
        /// <returns>Returns instance of <see cref="RoutePackage"/>.</returns>
        public async Task<RoutePackage> CreateRoutePackageAsync(RoutePackage package)
        {
            using (var db = new SubrouteContext())
            {
                // Try to get the user ID for auditing purposes.
                var userId = Thread.CurrentPrincipal.GetUserId();

                // Set auto property values.
                package.CreatedOn = DateTimeOffset.Now;
                package.CreatedBy = userId;
                package.UpdatedOn = package.CreatedOn;
                package.UpdatedBy = userId;

                db.RoutePackages.Add(package);
                await db.SaveChangesAsync();

                // Detach entry so its changes won't be tracked.
                db.Entry(package).State = EntityState.Detached;

                return package;
            }
        }

        /// <summary>
        /// Update an existing route package record using the specified <see cref="RoutePackage"/> instance.
        /// </summary>
        /// <param name="package">Instance of <see cref="RoutePackage"/> used to update database record.</param>
        /// <returns>The updated <see cref="RoutePackage"/> record.</returns>
        public async Task<RoutePackage> UpdateRoutePackageAsync(RoutePackage package)
        {
            using (var db = new SubrouteContext())
            {
                // Try to get the user ID for auditing purposes.
                var userId = Thread.CurrentPrincipal.GetUserId();
                
                var existingPackage = await db.RoutePackages.FirstOrDefaultAsync(rs => rs.Id == package.Id);

                if (existingPackage == null)
                    throw new NotFoundException($"No route package exists with ID '{package.Id}'.");
                
                existingPackage.Version = package.Version;
                existingPackage.UpdatedOn = DateTimeOffset.Now;
                existingPackage.UpdatedBy = userId;

                await db.SaveChangesAsync();

                // Detach entry so its changes won't be tracked.
                db.Entry(existingPackage).State = EntityState.Detached;

                return existingPackage;
            }
        }

        /// <summary>
        /// Delete the route package record with the specified identifier.
        /// </summary>
        /// <param name="identifier"><see cref="RouteIdentifier"/> for the requested route.</param>
        /// <param name="id">String that identifies the route package record to delete.</param>
        /// <returns>Returns a Task, essentially void when using async syntax.</returns>
        public async Task DeleteRoutePackageAsync(RouteIdentifier identifier, string id)
        {
            using (var db = new SubrouteContext())
            {
                var route = await GetRoutePackageByIdAsync(identifier, id);

                // Mark route for deletion so the context knowns to delete it.
                db.Entry(route).State = EntityState.Deleted;

                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Delete the route package record using the passed instance.
        /// </summary>
        /// <param name="package">Instance of <see cref="RoutePackage"/> that represents the record to delete.</param>
        /// <returns>Returns a Task, essentially void when using async syntax.</returns>
        public async Task DeleteRoutePackageAsync(RoutePackage package)
        {
            using (var db = new SubrouteContext())
            {
                // Mark route for deletion so the context knowns to delete it.
                db.Entry(package).State = EntityState.Deleted;

                await db.SaveChangesAsync();
            }
        }
    }
}