using System.Web.Http;
using Subroute.Api;
using Subroute.Api.Handlers;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(RouteConfig), "Register")]
namespace Subroute.Api
{
    public class RouteConfig
    {
        public static void Register()
        {
            var configuration = GlobalConfiguration.Configuration;

            // We'll redirect root requests to the swagger UI as a helper to users of our API.
            configuration.Routes.MapHttpRoute("Default", "", new { }, null, new SwaggerHandler());

            // We'll map all other routes using action attributes.
            configuration.MapHttpAttributeRoutes();
        }
    }
}