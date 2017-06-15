using System.Web.Configuration;
using System.Web.Http;
using Subroute.Api;
using Subroute.Api.Handlers;
using Subroute.Core.Compression;
using Subroute.Core.Tracing;
using WebActivatorEx;

// PostApplicationStartMethod will run after the first HttpModule gets initialized, which by this time will have
// the dependency resolver registered and configured.
[assembly: PostApplicationStartMethod(typeof(HandlerConfig), "Register")]
namespace Subroute.Api
{
    public class HandlerConfig
    {
        public static void Register()
        {
            // We must resolve an instance of IAuthenticationProvider because our token authentication filter depends on it.
            var configuration = GlobalConfiguration.Configuration;

            var clientId = WebConfigurationManager.AppSettings["auth0:ClientId"];
            var clientSecret = WebConfigurationManager.AppSettings["auth0:ClientSecret"];
            
            configuration.MessageHandlers.Add(new CompressionHandler());
            configuration.MessageHandlers.Add(new TracingHandler());
            configuration.MessageHandlers.Add(new SecureHandler());
            configuration.MessageHandlers.Add(new JsonWebTokenValidationHandler
            {
                Audience = clientId,
                SymmetricKey = clientSecret
            });
        }
    }
}