using System.Collections.Generic;

namespace Subroute.App.Properties
{
    /// <summary>
    /// Settings that are used to determine how to output the client side settings module.
    /// </summary>
    public class ClientSettingHandlerOptions
    {
        /// <summary>
        /// Gets or sets the module type of the outputted settings.
        /// Amd - Create a module that is compatible with loaders such as RequireJS.
        /// Json - Create a module that contains a basic json structure containing settings.
        /// </summary>
        public ClientSettingModuleType ModuleType { get; set; }

        /// <summary>
        /// Gets or sets whether to remove the namespace that was used to locate the settings from the output module.
        /// </summary>
        public bool RemoveNamespaces { get; set; }

        /// <summary>
        /// Gets or sets whether to camel case the property names when generating the output module.
        /// </summary>
        public bool CamelCasePropertyNames { get; set; }

        /// <summary>
        /// Gets or sets a list of namespaces to use with locating app settings. Uses basic "starts with" logic to match.
        /// </summary>
        public List<string> Namespaces { get; set; } 
    }
}