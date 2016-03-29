using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Subroute.Common;
using Subroute.Core.Execution;
using Subroute.Core.Tracing;

namespace Subroute.Core
{
    public static class Bootstrapper
    {
        public static ContainerBuilder GetContainerBuilder(params Assembly[] assemblies)
        {
            var builder = new ContainerBuilder();
            var assemblyArray = assemblies.Concat(new[] {typeof (Bootstrapper).Assembly}).ToArray();

            builder.RegisterAssemblyTypes(assemblyArray)
                .Where(a => a.Name.EndsWith("Provider"))
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assemblyArray)
                .Where(a => a.Name.EndsWith("Handler"))
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assemblyArray)
                .Where(a => a.Name.EndsWith("Factory"))
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assemblyArray)
                .Where(a => a.Name.EndsWith("Publisher"))
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assemblyArray)
                .Where(a => a.Name.EndsWith("Repository"))
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assemblyArray)
                .Where(a => a.Name.EndsWith("Service"))
                .AsImplementedInterfaces();

            builder.RegisterType<TraceContext>()
                .As<ITraceContext>()
                .InstancePerLifetimeScope();

            // ResponsePipeline must be a single instance or we'll get multiple listeners on the same subscription
            // and only one would receive it, so you'd never know if you were subscribed to the correct observable.
            builder.RegisterType<ResponsePipeline>()
                .As<IResponsePipeline>()
                .SingleInstance();
             
            return builder;
        }
    }
}
