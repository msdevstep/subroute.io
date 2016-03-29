using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Autofac.Integration.WebApi;
using Microsoft.ServiceBus.Messaging;
using Subroute.Api;
using Subroute.Core;
using Subroute.Core.Exceptions;
using Subroute.Core.ServiceBus;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(ServiceBusConfig), "Register")]
namespace Subroute.Api
{
    public class ServiceBusConfig
    {
        public static void Register()
        {
            var container = GlobalConfiguration.Configuration.DependencyResolver;
            var topicFactory = (ITopicFactory) container.GetService(typeof (ITopicFactory));

            topicFactory.CreateTopicAsync(Settings.ServiceBusRequestTopicName).Wait();
            topicFactory.CreateTopicAsync(Settings.ServiceBusResponseTopicName).Wait();
            topicFactory.CreateSubscriptionAsync(Settings.ServiceBusRequestTopicName, Settings.ServiceBusRequestSubscriptionName).Wait();

            // Create a subscription that uses a correlation filter for this specific machine instance by machine name.
            var requestSubscriptionName = string.Format(Settings.ServiceBusResponseSubscriptionNameFormat, Environment.MachineName);
            var requestSubscriptionFilter = new CorrelationFilter(Environment.MachineName);
            topicFactory.CreateSubscriptionAsync(Settings.ServiceBusResponseTopicName, requestSubscriptionName, requestSubscriptionFilter).Wait();
        }
    }
}