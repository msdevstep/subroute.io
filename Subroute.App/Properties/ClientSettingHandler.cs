using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Subroute.App.Properties
{
    /// <summary>
    /// For use with <see cref="HttpRouteCollection" />.MapHttpRoute(). Will generate a virtual
    /// file containing json or an AMD javascript module. The file will contain any properties
    /// from the AppSettings section of the web.config that begin with the specified
    /// namespaces. This is useful to enable the client-side settings to be updated for
    /// different deployment environments in the same way the server settings are.
    /// </summary>
    public class ClientSettingHandler : HttpMessageHandler
    {
        private readonly ClientSettingHandlerOptions _options;

        /// <summary>
        /// Instantiate a new <see cref="ClientSettingHandler" /> with the specified <see cref="ClientSettingHandlerOptions" />.
        /// </summary>
        /// <param name="options">Options that modify the way the settings are outputted.</param>
        public ClientSettingHandler(ClientSettingHandlerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Executed by the WebApi framework when the mapped route is requested.
        /// </summary>
        /// <param name="request">Request containing all details related to the current request.</param>
        /// <param name="cancellationToken">Cancellation token for the current request chain.</param>
        /// <returns>Task containing the generated HttpResponseMessage.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Locate all app settings from web.config that start with the provided namespaces.
            // Once we have our list of setting keys, we'll use them to create a dictionary of
            // keys without the namespaces used to search (if requested).
            var appSettings = ConfigurationManager.AppSettings;
            var settings = appSettings.AllKeys.Where(k => _options.Namespaces.Any(k.StartsWith) && !k.EndsWith("|Resource", StringComparison.CurrentCultureIgnoreCase));
            var resources = appSettings.AllKeys.Where(k => _options.Namespaces.Any(k.StartsWith) && k.EndsWith("|Resource", StringComparison.CurrentCultureIgnoreCase));
            var payload = settings.ToDictionary(RemoveNamespaces, s => InferType(appSettings[s]));

            // Load each resource and add them individually.
            foreach (var resource in resources)
            {
                var name = appSettings[resource];
                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream(name))
                {
                    if (stream == null)
                        throw new NullReferenceException($"Could not load embedded resource named '{name}'.");

                    using (var reader = new StreamReader(stream))
                        payload.Add(RemoveNamespaces(resource), await reader.ReadToEndAsync());
                }
            }

            // We need a JsonSerializerSettings class to hold the CamelCasePropertyNamesContractResolver if
            // the developer has requested that the outputted property names should be camel cased.
            var serializerSettings = new JsonSerializerSettings();

            if (_options.CamelCasePropertyNames)
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Use settings to serialize our dictionary of settings. This will create a flat JSON object.
            var output = JsonConvert.SerializeObject(payload, serializerSettings);
            var contentType = "application/json";

            // Depending on the type of file the developer wants to output, we should check for AMD and
            // if requested, wrap our JSON with the AMD module syntax so that it'll be understood on the client.
            if (_options.ModuleType == ClientSettingModuleType.Amd)
            {
                contentType = "application/javascript";
                output = $"define(function () {{ return {output}; }});";
            }

            // We have our final output, lets prepare a 200 response containing the
            // raw string value with the requested content type (for module).
            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(output, Encoding.UTF8, contentType);
            return response;
        }

        private static object InferType(string value)
        {
            // Infer boolean types.
            bool output;
            if (bool.TryParse(value, out output))
                return output;

            return value;
        }

        private string RemoveNamespaces(string setting)
        {
            // Only remove the namespaces if the developer has requested it.
            if (!_options.RemoveNamespaces)
                return setting;

            // Remove any type indicators from the end.
            var pipeIndex = setting.LastIndexOf("|", StringComparison.CurrentCultureIgnoreCase);
            if (pipeIndex > -1)
                setting = setting.Substring(0, pipeIndex);

            // Iterate over each namespace and one-by-one check if the namespace needs removed
            // from the specified setting and if so, remove it. When all namespaces have been
            // processed, the final result will be trimmed of '.' that may be left over
            // from the removal of a namespace segment.
            return _options.Namespaces.Aggregate(setting,
                (working, ns) => working.StartsWith(ns) ? working.Substring(ns.Length) : working,
                final => final.Trim('.'));
        }
    }
}