using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Subroute.Core.Data;

namespace Subroute.Api.Models.RouteSettings
{
    public class RouteSettingRequest
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public static Expression<Func<RouteSettingRequest, int, RouteSetting>> Projection = (r, i) => r == null ? null : new RouteSetting
        {
            RouteId = i,
            Name = r.Name,
            Value = r.Value
        };

        public static Func<RouteSettingRequest, int, RouteSetting> Map = Projection.Compile();
    }
}
