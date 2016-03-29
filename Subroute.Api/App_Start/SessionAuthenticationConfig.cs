using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Subroute.Api;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(SessionAuthenticationConfig), "PreAppStart")]

namespace Subroute.Api
{
    public static class SessionAuthenticationConfig
    {
        public static void PreAppStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(System.IdentityModel.Services.SessionAuthenticationModule));
        }
    }
}