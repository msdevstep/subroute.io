using System.Web.Http;
using System.Web.Http.Cors;
using Subroute.Api;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(CrossOriginConfig), "Register")]
namespace Subroute.Api
{
    public class CrossOriginConfig
    {
        public static void Register()
        {
            var configuration = GlobalConfiguration.Configuration;
            configuration.EnableCors(new EnableCorsAttribute("*", "*", "*"));
        }
    }
}