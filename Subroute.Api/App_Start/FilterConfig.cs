using System.Web.Http;
using Subroute.Api;
using Subroute.Api.Filters;
using WebActivatorEx;

// PostApplicationStartMethod will run after the first HttpModule gets initialized, which by this time will have
// the dependency resolver registered and configured.
[assembly: PostApplicationStartMethod(typeof(FilterConfig), "Register")]
namespace Subroute.Api
{
    public class FilterConfig
    {
        public static void Register()
        {
            var configuration = GlobalConfiguration.Configuration;

            configuration.Filters.Add(new StatusCodeExceptionFilterAttribute());
            configuration.Filters.Add(new AuthorizeAttribute());
        }
    }
}