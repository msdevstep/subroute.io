using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subroute.Core.Data
{
    public class SubrouteContext : DbContext
    {
        public SubrouteContext()
            : base(Settings.ConnectionString)
        {
            
        }

        public DbSet<Route> Routes { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<RouteSetting> RouteSettings { get; set; }
        public DbSet<RoutePackage> RoutePackages { get; set; }

        public static void InitializeMetadata()
        {
            using (var db = new SubrouteContext())
            {
                // Hack: Perform a no-op query to force EntityFramework to load metadata (which can take a while).
                var routes = db.Routes.Take(0).ToArray();
            }
        }
    }
}
