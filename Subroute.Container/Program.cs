using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Subroute.Core;
using Subroute.Core.Data;
using Subroute.Core.Data.Repositories;
using Subroute.Core.Utilities;

namespace Subroute.Container
{
    class Program
    {
        public static IContainer Container = null;
        public static IRequestRepository RequestRepository = null;

        static void Main(string[] args)
        {
            // Wire tracing sub-system into console output.
            Trace.Listeners.Add(new ConsoleTraceListener());

            // Register all dependencies once during app setup. This will ensure dependency registration happens
            // well before the first request is received. Previously I was registering dependencies in the
            // ExecutionMethods class, but that instance doesn't get created until the first request.
            TraceUtility.TraceTime("Register Dependencies", () =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var builder = Bootstrapper.GetContainerBuilder(assembly);

                Container = builder.Build();
            });

            // We'll also get an instance of the RequestRepository. Since the RequestRepository is thread-safe
            // we'll register a single instance of it here to reduce the amount of operations we perform during
            // the execution of an individual request.
            RequestRepository = Container.Resolve<IRequestRepository>();

            // Force the database to initialize Entity Framework metadata.
            // It turns out Entity Framework lazy-loads all its metadata
            // and this make the first request take many seconds to run.
            // The initialization performs a no-op query which warms EF up.
            TraceUtility.TraceTime("Initialize Entity Framework", () =>
            {
                // Assign a custom DatabaseInitializer to configure certain aspects of our database.
                Database.SetInitializer(new DatabaseInitializer());

                SubrouteContext.InitializeMetadata();
            });

            var jobHostConfig = new JobHostConfiguration
            {
                NameResolver = new ConfigNameResolver(),
                DashboardConnectionString = Settings.StorageConnectionString,
                StorageConnectionString = Settings.StorageConnectionString
            };

            var serviceBusConfig = new ServiceBusConfiguration
            {
                ConnectionString = Settings.ServiceBusConnectionString,
                MessageOptions = new OnMessageOptions
                {
                    MaxConcurrentCalls = Settings.ServiceBusMaxConcurrentExecutions
                }
            };

            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Tcp;

            jobHostConfig.UseServiceBus(serviceBusConfig);

            using (var jobHost = new JobHost(jobHostConfig))
                jobHost.RunAndBlock();
        }
    }
}
