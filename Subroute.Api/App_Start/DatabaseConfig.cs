using System.Web.Http;
using Subroute.Api;
using Subroute.Api.Filters;
using Subroute.Core.Data;
using WebActivatorEx;

// PostApplicationStartMethod will run after the first HttpModule gets initialized, which by this time will have
// the dependency resolver registered and configured.
[assembly: PostApplicationStartMethod(typeof(DatabaseConfig), "Register")]
namespace Subroute.Api
{
    public class DatabaseConfig
    {
        public static void Register()
        {
            // This method will perform a no-op query against the database and will force
            // Entity Framework to initialize its metadata, which is usually done on the
            // first database request. The initialization can take over 6 seconds to run
            // which means that if we don't pre-warm-up Entity Framework, the first user
            // will have to wait for this call.
            SubrouteContext.InitializeMetadata();
        }
    }
}