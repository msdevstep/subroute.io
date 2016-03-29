using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Subroute.Api;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SerializerConfig), "Register")]
namespace Subroute.Api
{
    public class SerializerConfig
    {
        public static void Register()
        {
            var configuration = GlobalConfiguration.Configuration;

            // We only want to allow JSON as our wire format.
            configuration.Formatters.Clear();
            configuration.Formatters.Add(new JsonMediaTypeFormatter());

            configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            configuration.Formatters.JsonFormatter.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
        }
    }
}