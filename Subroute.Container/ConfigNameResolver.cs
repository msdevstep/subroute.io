using System;
using System.Configuration;
using Microsoft.Azure.WebJobs;

namespace Subroute.Container
{
    /// <summary>
    /// Can be used to resolve % % tokens from the config file
    /// </summary>
    internal class ConfigNameResolver : INameResolver
    {
        public string Resolve(string name)
        {
            var resolvedName = ConfigurationManager.AppSettings[name];

            if (string.IsNullOrWhiteSpace(resolvedName))
                throw new InvalidOperationException("Cannot resolve " + name);

            return resolvedName;
        }
    }
}