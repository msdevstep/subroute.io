using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Subroute.App.Properties;

namespace Subroute.App
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // We'll create a virtual route and use the ClientSettingProvider to dynamically generate a settings
            // file based on settings pulled from the web.config file whos names start with "Client".
            var options = new ClientSettingHandlerOptions
            {
                Namespaces = new List<string> { "Client" },
                ModuleType = ClientSettingModuleType.Amd,
                CamelCasePropertyNames = true,
                RemoveNamespaces = true
            };

            // Register a virtual route at http://environment.local/app/config that will output an AMD
            // compliant module containing all app settings from the web.config that start with "Client".
            GlobalConfiguration.Configuration.Routes.MapHttpRoute("ClientSettings", "app/config", null, null, new ClientSettingHandler(options));

            // Map all other routes using RouteAttribute on each controller action,
            // then finish  initialization of our WebApi configuration.
            GlobalConfiguration.Configuration.MapHttpAttributeRoutes();
            GlobalConfiguration.Configuration.EnsureInitialized();

            // Configure MVC route for default page.
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}