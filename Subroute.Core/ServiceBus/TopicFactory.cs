using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Subroute.Core.ServiceBus
{
    public interface ITopicFactory
    {
        Task CreateTopicAsync(string topicName);
        Task CreateSubscriptionAsync(string topicName, string subscriptionName);
        Task CreateSubscriptionAsync(string topicName, string subscriptionName, Filter filter);
        Task DeleteSubscriptionAsync(string topicName, string subscriptionName);
        Task<TopicClient> CreateTopicClientAsync(string topicName, bool ensureExists = false);
        Task<SubscriptionClient> CreateSubscriptionClientAsync(string topicName, string subscriptionName, bool ensureExists = false);
        Task<SubscriptionClient> CreateSubscriptionClientAsync(string topicName, string subscriptionName, Filter filter, bool ensureExists = false);
    }

    public class TopicFactory : ITopicFactory
    {
        public async Task CreateTopicAsync(string topicName)
        {
            var topicDescription = new TopicDescription(topicName)
            {
                MaxSizeInMegabytes = 5120,
                EnablePartitioning = true,
                EnableExpress = true,
                RequiresDuplicateDetection = false
            };

            var namespaceManager = NamespaceManager.CreateFromConnectionString(Settings.ServiceBusConnectionString);

            if (!await namespaceManager.TopicExistsAsync(topicName))
                await namespaceManager.CreateTopicAsync(topicDescription);
        }

        public async Task<TopicClient> CreateTopicClientAsync(string topicName, bool ensureExists = false)
        {
            if (ensureExists)
                await CreateTopicAsync(topicName);

            return TopicClient.CreateFromConnectionString(Settings.ServiceBusConnectionString, topicName);
        }

        public async Task CreateSubscriptionAsync(string topicName, string subscriptionName)
        {
            await CreateSubscriptionAsync(topicName, subscriptionName, new TrueFilter());
        }

        public async Task CreateSubscriptionAsync(string topicName, string subscriptionName, Filter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            var namespaceManager = NamespaceManager.CreateFromConnectionString(Settings.ServiceBusConnectionString);

            if (!await namespaceManager.SubscriptionExistsAsync(topicName, subscriptionName))
            {
                var subscriptionDescription = new SubscriptionDescription(topicName, subscriptionName)
                {
                    LockDuration = TimeSpan.FromMinutes(5)
                };
                
                await namespaceManager.CreateSubscriptionAsync(subscriptionDescription, filter);
            }
        }

        public async Task DeleteSubscriptionAsync(string topicName, string subscriptionName)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(Settings.ServiceBusConnectionString);
            
            if (await namespaceManager.SubscriptionExistsAsync(topicName, subscriptionName))
                await namespaceManager.DeleteSubscriptionAsync(topicName, subscriptionName);
        }

        public Task<SubscriptionClient> CreateSubscriptionClientAsync(string topicName, string subscriptionName, bool ensureExists = false)
        {
            return CreateSubscriptionClientAsync(topicName, subscriptionName, new TrueFilter(), ensureExists);
        }

        public async Task<SubscriptionClient> CreateSubscriptionClientAsync(string topicName, string subscriptionName, Filter filter, bool ensureExists = false)
        {
            if (ensureExists)
                await CreateSubscriptionAsync(topicName, subscriptionName, filter);

            return SubscriptionClient.CreateFromConnectionString(Settings.ServiceBusConnectionString, topicName, subscriptionName, ReceiveMode.ReceiveAndDelete);
        }
    }
}
