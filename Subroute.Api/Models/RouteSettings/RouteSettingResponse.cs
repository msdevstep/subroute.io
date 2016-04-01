using System;
using System.Linq.Expressions;
using Subroute.Api.Models.Routes;
using Subroute.Core.Data;

namespace Subroute.Api.Models.RouteSettings
{
    public class RouteSettingResponse
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        
        public static Expression<Func<RouteSetting, RouteSettingResponse>> Projection = r => r == null ? null : new RouteSettingResponse
        {
            Name = r.Name,
            Value = r.Value,
            CreatedBy = r.CreatedBy,
            CreatedOn = r.CreatedOn,
            UpdatedBy = r.UpdatedBy,
            UpdatedOn = r.UpdatedOn
        };

        public static Func<RouteSetting, RouteSettingResponse> Map = Projection.Compile();
    }
}