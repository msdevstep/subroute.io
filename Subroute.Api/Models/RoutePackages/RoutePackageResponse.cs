using System;
using System.Linq.Expressions;
using Subroute.Api.Models.Routes;
using Subroute.Core.Data;

namespace Subroute.Api.Models.RoutePackages
{
    public class RoutePackageResponse
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        
        public static Expression<Func<RoutePackage, RoutePackageResponse>> Projection = r => r == null ? null : new RoutePackageResponse
        {
            Id = r.Id,
            Version = r.Version,
            CreatedBy = r.CreatedBy,
            CreatedOn = r.CreatedOn,
            UpdatedBy = r.UpdatedBy,
            UpdatedOn = r.UpdatedOn
        };

        public static Func<RoutePackage, RoutePackageResponse> Map = Projection.Compile();
    }
}