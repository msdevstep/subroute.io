using System.Security.Claims;
using System.Security.Principal;
using Newtonsoft.Json;
using Subroute.Core.Exceptions;

namespace Subroute.Core.Extensions
{
    public static class PrincipalExtensions
    {
        public static ClaimsPrincipal GetClaimsPrincipal(this IPrincipal principal)
        {
            var current = ClaimsPrincipal.Current;

            if (current == null)
                throw new AuthenticationException("No user is currently authenticated. Please authenticate and try again.");

            return current;
        }

        public static string GetUserId(this IPrincipal principal)
        {
            return principal.GetClaim("user_id");
        }

        public static string GetEmail(this IPrincipal principal)
        {
            return principal.GetClaim("email");
        }

        public static string GetName(this IPrincipal principal)
        {
            return principal.GetClaim("name");
        }

        public static string GetPictureUri(this IPrincipal principal)
        {
            return principal.GetClaim("picture");
        }

        public static string GetFirstName(this IPrincipal principal)
        {
            return principal.GetClaim("given_name");
        }

        public static string GetLastName(this IPrincipal principal)
        {
            return principal.GetClaim("family_name");
        }

        public static bool IsAdmin(this IPrincipal principal)
        {
            var claim = principal.GetClaim("user_metadata");

            if (claim == null)
                return false;

            var anon = new {IsAdmin = false};
            var deserialized = JsonConvert.DeserializeAnonymousType(claim, anon);
            return deserialized.IsAdmin;
        }
        
        public static string GetClaim(this IPrincipal principal, string name)
        {
            var current = principal.GetClaimsPrincipal();
            var claim = current.FindFirst(name);

            return claim?.Value;
        }
    }
}