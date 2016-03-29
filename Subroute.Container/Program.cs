using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Subroute.Core;

namespace Subroute.Container
{
    class Program
    {
        static void Main(string[] args)
        {
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

            var jobHost = new JobHost(jobHostConfig);
            jobHost.RunAndBlock();
        }
    }
}
