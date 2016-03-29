using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subroute.Core
{
    public static class Settings
    {
        private static readonly IDictionary<string, object> SettingsCache = new ConcurrentDictionary<string, object>();

        public static string ConnectionString => GetSetting<string>("Subroute.ConnectionString");
        public static string StorageConnectionString => GetSetting<string>("Subroute.Storage.ConnectionString");
        public static string ApiBaseUri => GetSetting<string>("Subroute.ApiBaseUri");
        public static string ServiceBusConnectionString => GetSetting<string>("Subroute.ServiceBus.ConnectionString");
        public static int ServiceBusMaxConcurrentExecutions => GetSetting<int>("Subroute.ServiceBus.MaxConcurrentExecutions");
        public static string ServiceBusRequestTopicName => GetSetting<string>("Subroute.ServiceBus.RequestTopicName");
        public static string ServiceBusResponseTopicName => GetSetting<string>("Subroute.ServiceBus.ResponseTopicName");
        public static string ServiceBusRequestSubscriptionName => GetSetting<string>("Subroute.ServiceBus.RequestSubscriptionName");
        public static string ServiceBusResponseSubscriptionNameFormat => GetSetting<string>("Subroute.ServiceBus.ResponseSubscriptionNameFormat");
        public static int ServiceBusResponseTimeoutMinutes => GetSetting<int>("Subroute.ServiceBus.ResponseTimeoutMinutes");
        public static string SubrouteAuth0ManagementApiUri => GetSetting<string>("Subroute.Auth0.ManagementApiUri");
        public static string SubrouteAuth0ManagementApiToken => GetSetting<string>("Subroute.Auth0.ManagementApiToken");
        public static string MailChimpApiKey => GetSetting<string>("Subroute.MailChimp.API.Key");
        public static string MailChimpAllSubscriberListId => GetSetting<string>("Subroute.MailChimp.All.Subscriber.ListId");
        public static string AppInsightsInstrumentationKey => GetSetting<string>("Subroute.AppInsights.InstrumentationKey");

        /// <summary>
        /// Get a strongly-typed setting value from the active environments configuration provider.
        /// </summary>
        /// <typeparam name="T">Type of setting value to return.</typeparam>
        /// <param name="name">Name of setting value to return.</param>
        /// <returns>Setting value as <typeparamref name="T"/></returns>
        public static T GetSetting<T>(string name)
        {
            // Attempt to find setting in cache first.
            object value;
            if (SettingsCache.TryGetValue(name, out value))
                return (T)value;

            var setting = ConfigurationManager.AppSettings[name];
            var typedValue = (T)Convert.ChangeType(setting, typeof(T));

            SettingsCache.Add(name, typedValue);

            return typedValue;
        }
    }
}
