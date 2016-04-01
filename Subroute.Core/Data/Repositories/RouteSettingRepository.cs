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
    public interface IRouteSettingRepository
    {
        Task<RouteSetting[]> GetRouteSettingsAsync(RouteIdentifier identifier);
        Task<RouteSetting> GetRouteSettingByNameAsync(string name);
        Task<RouteSetting> CreateRouteSettingAsync(RouteSetting setting);
        Task<RouteSetting> UpdateRouteSettingAsync(RouteSetting setting);
        Task DeleteRouteSettingAsync(string name);
        Task DeleteRouteSettingAsync(RouteSetting setting);
    }

    public class RouteSettingRepository : IRouteSettingRepository
    {
        /// <summary>
        /// Load all the route settings for the specified <see cref="RouteIdentifier"/>.
        /// </summary>
        /// <param name="identifier"><see cref="RouteIdentifier"/> for the requested route.</param>
        /// <returns>Returns an array of <see cref="RouteSetting"/> objects for the specified route.</returns>
        public async Task<RouteSetting[]> GetRouteSettingsAsync(RouteIdentifier identifier)
        {
            using (var db = new SubrouteContext())
            {
                // Get an untracked base queryable.
                var query = db.RouteSettings
                    .AsNoTracking()
                    .AsQueryable();

                // Ensure we are only returning settings for the specified route
                // Use the correct identifier type to query the database.
                query = identifier.Type == RouteIdentifierType.Id
                    ? query.Where(r => r.RouteId == identifier.Id)
                    : query.Where(r => r.Route.Uri == identifier.Uri);

                // Materialize the results into memory.
                return await query.ToArrayAsync();
            }
        }

        /// <summary>
        /// Load a single route setting by its name.
        /// </summary>
        /// <param name="name">String representing the specified route setting name.</param>
        /// <returns>Returns a single <see cref="RouteSetting"/> with the matching route setting identifier.</returns>
        public async Task<RouteSetting> GetRouteSettingByNameAsync(string name)
        {
            using (var db = new SubrouteContext())
                return await db.RouteSettings.FirstOrDefaultAsync(rs => rs.Name == name);
        }

        /// <summary>
        /// Uses the specified <see cref="RouteSetting"/> instance to create a new route setting record in the database.
        /// </summary>
        /// <param name="setting">Instance of <see cref="RouteSetting"/> used to create the database record.</param>
        /// <returns>Returns instance of <see cref="RouteSetting"/>.</returns>
        public async Task<RouteSetting> CreateRouteSettingAsync(RouteSetting setting)
        {
            using (var db = new SubrouteContext())
            {
                // Try to get the user ID for auditing purposes.
                var userId = Thread.CurrentPrincipal.GetUserId();

                // Set auto property values.
                setting.CreatedOn = DateTimeOffset.Now;
                setting.CreatedBy = userId;
                setting.UpdatedOn = setting.CreatedOn;
                setting.UpdatedBy = userId;

                db.RouteSettings.Add(setting);
                await db.SaveChangesAsync();

                // Detach entry so its changes won't be tracked.
                db.Entry(setting).State = EntityState.Detached;

                return setting;
            }
        }

        /// <summary>
        /// Update an existing route setting record using the specified <see cref="RouteSetting"/> instance.
        /// </summary>
        /// <param name="setting">Instance of <see cref="RouteSetting"/> used to update database record.</param>
        /// <returns>The updated <see cref="RouteSetting"/> that includes the generated route setting ID.</returns>
        public async Task<RouteSetting> UpdateRouteSettingAsync(RouteSetting setting)
        {
            using (var db = new SubrouteContext())
            {
                // Try to get the user ID for auditing purposes.
                var userId = Thread.CurrentPrincipal.GetUserId();
                
                var existingSetting = await db.RouteSettings.FirstOrDefaultAsync(rs => rs.Name == setting.Name);

                if (existingSetting == null)
                    throw new NotFoundException($"No route setting exists with name '{setting.Name}'.");
                
                existingSetting.Value = setting.Value;
                existingSetting.UpdatedOn = DateTimeOffset.Now;
                existingSetting.UpdatedBy = userId;

                await db.SaveChangesAsync();

                // Detach entry so its changes won't be tracked.
                db.Entry(existingSetting).State = EntityState.Detached;

                return existingSetting;
            }
        }

        /// <summary>
        /// Delete the route setting record with the specified identifier.
        /// </summary>
        /// <param name="name">String that identifies the route setting record to delete.</param>
        /// <returns>Returns a Task, essentially void when using async syntax.</returns>
        public async Task DeleteRouteSettingAsync(string name)
        {
            using (var db = new SubrouteContext())
            {
                var route = await GetRouteSettingByNameAsync(name);

                // Mark route for deletion so the context knowns to delete it.
                db.Entry(route).State = EntityState.Deleted;

                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Delete the route setting record using the passed instance.
        /// </summary>
        /// <param name="setting">Instance of <see cref="RouteSetting"/> that represents the record to delete.</param>
        /// <returns>Returns a Task, essentially void when using async syntax.</returns>
        public async Task DeleteRouteSettingAsync(RouteSetting setting)
        {
            using (var db = new SubrouteContext())
            {
                // Mark route for deletion so the context knowns to delete it.
                db.Entry(setting).State = EntityState.Deleted;

                await db.SaveChangesAsync();
            }
        }
    }
}