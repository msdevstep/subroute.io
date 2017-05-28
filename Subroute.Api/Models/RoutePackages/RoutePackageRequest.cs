using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Subroute.Core.Data;

namespace Subroute.Api.Models.RoutePackages
{
    public class RoutePackageRequest
    {
        public string Id { get; set; }
        public string Version { get; set; }

        public static Expression<Func<RoutePackageRequest, int, RoutePackage>> Projection = (r, i) => r == null ? null : new RoutePackage
        {
            RouteId = i,
            Id = r.Id,
            Version = r.Version
        };

        public static Func<RoutePackageRequest, int, RoutePackage> Map = Projection.Compile();
    }
}
