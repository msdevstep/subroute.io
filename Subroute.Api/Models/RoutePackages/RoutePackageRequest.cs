using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Subroute.Core.Data;
using Subroute.Core.Models.Compiler;

namespace Subroute.Api.Models.RoutePackages
{
    public class RoutePackageRequest
    {
        public string Id { get; set; }
        public string Version { get; set; }

        /// <summary>
        /// Converts the current <see cref="RoutePackageRequest"/> instance to a new instance of <see cref="Dependency"/>.
        /// </summary>
        /// <returns>Instance of <see cref="Dependency"/> with the current Id and Version.</returns>
        public Dependency ToDependency()
        {
            return new Dependency
            {
                Id = Id,
                Version = Version
            };
        }


        public static Expression<Func<RoutePackageRequest, int, RoutePackage>> Projection = (r, i) => r == null ? null : new RoutePackage
        {
            RouteId = i,
            Id = r.Id,
            Version = r.Version
        };

        public static Func<RoutePackageRequest, int, RoutePackage> Map = Projection.Compile();
    }
}
