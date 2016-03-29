using System.ComponentModel.DataAnnotations;
using System.Threading;
using Subroute.Core.Data;
using Subroute.Core.Extensions;
using Subroute.Core.Models.Routes;

namespace Subroute.Api.Models.Routes
{
    public class RouteRequest
    {
        public string UserId { get; set; }
        public string Uri { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public bool IsDefault { get; set; }

        public Route Patch(Route route)
        {
            route.UserId = UserId;
            route.Title = Title;
            route.Uri = Uri;
            route.Code = Code;

            // IsDefault can only be set by an admin user.
            if (Thread.CurrentPrincipal.IsAdmin())
                route.IsDefault = IsDefault;

            return route;
        }
    }
}