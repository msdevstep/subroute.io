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
    }
}
