using System.Reflection;
using System.Web.Http;
using Autofac.Integration.WebApi;
using Subroute.Api;
using Subroute.Core;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(DependencyConfig), "Register")]
namespace Subroute.Api
{
    public class DependencyConfig
    {
        public static void Register()
        {
            var configuration = GlobalConfiguration.Configuration;
            var assembly = Assembly.GetExecutingAssembly();
            var builder = Bootstrapper.GetContainerBuilder(assembly);

            // Todo: Register types from this assembly that aren't already registered by convention.

            // We need to register all web api related components.
            builder.RegisterApiControllers(assembly);
            builder.RegisterWebApiModelBinders(assembly);
            builder.RegisterWebApiFilterProvider(configuration);

            // We need a container to actually resolve types, so use the builder we populated with registrations
            // to create a valid container for us to pass as our dependency resolver.
            var container = builder.Build();

            // Use AutofacWebApiDependencyResolver as an adapter between web api's resolver and our container.
            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}