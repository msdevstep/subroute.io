using System.Web.Mvc;
using System.Web.Routing;

namespace Subroute.App.Properties
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.Ignore("{resourcedir}/{*resource}", new { resourcedir = @"(api|breeze|app|scripts|content|assets|signalr)" });

            routes.MapRoute(
                name: "Default",
                url: "{*sparoute}",
                defaults: new { controller = "Default", action = "Index" }
            );
        }
    }
}