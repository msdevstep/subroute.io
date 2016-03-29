using System;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Subroute.Core.Data;
using Subroute.Core.Models.Routes;

namespace Subroute.Api.Models.Routes
{
    public class RouteResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Uri { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public bool IsOnline { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsDefault { get; set; }
        public DateTimeOffset? ClonedOn { get; set; }
        public DateTimeOffset? PublishedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int StarredCount { get; set; }
        public int ClonedCount { get; set; }

        public static Expression<Func<Route, RouteResponse>> Projection = r => r == null ? null : new RouteResponse
        {
            Id = r.Id,
            Uri = r.Uri,
            Title = r.Title,
            UserId = r.UserId,
            Code = r.Code,
            IsCurrent = r.IsCurrent,
            IsPrivate = r.IsPrivate,
            IsDefault = r.IsDefault,
            ClonedCount = r.ClonedCount,
            StarredCount = r.StarredCount,
            IsOnline = r.IsOnline,
            ClonedOn = r.ClonedOn,
            PublishedOn = r.PublishedOn,
            CreatedBy = r.CreatedBy,
            CreatedOn = r.CreatedOn,
            UpdatedBy = r.UpdatedBy,
            UpdatedOn = r.UpdatedOn
        };

        public static Func<Route, RouteResponse> Map = Projection.Compile();
    }
}