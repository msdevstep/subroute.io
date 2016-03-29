using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subroute.Core.Data
{
    public class DatabaseInitializer : MigrateDatabaseToLatestVersion<SubrouteContext, SubrouteMigrationConfiguration>
    {

    }

    public class SubrouteMigrationConfiguration : DbMigrationsConfiguration<SubrouteContext>
    {
        public SubrouteMigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }
    }
}
