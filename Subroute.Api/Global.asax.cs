using System.Data.Entity;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Autofac.Integration.WebApi;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ServiceBus;
using Subroute.Core;
using Subroute.Core.Data;
using Subroute.Core.ServiceBus;

namespace Subroute.Api
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Tcp;

            // Assign a custom DatabaseInitializer to configure certain aspects of our database.
            Database.SetInitializer(new DatabaseInitializer());

            // Setup AppInsights instrumentation key.
            TelemetryConfiguration.Active.InstrumentationKey = Settings.AppInsightsInstrumentationKey;

            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
            GlobalConfiguration.Configuration.EnsureInitialized();
        }
    }
}