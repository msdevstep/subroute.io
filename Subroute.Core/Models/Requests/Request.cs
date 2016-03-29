using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Subroute.Core.Utilities;

namespace Subroute.Core.Models.Requests
{
    public class Request
    {
        public int Id { get; set; }
        public string Method { get; set; }
        public string Uri { get; set; }
        public int? StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public IDictionary<string, string> RequestHeaders { get; set; }
        public IDictionary<string, string> ResponseHeaders { get; set; }
        public long RequestLength { get; set; }
        public long? ResponseLength { get; set; }
        public string IpAddress { get; set; }
        public DateTimeOffset OccurredOn { get; set; }
        public DateTimeOffset? CompletedOn { get; set; }
        [JsonIgnore]
        public string SerializedRequestHeaders { get; set; }
        [JsonIgnore]
        public string SerializedResponseHeaders { get; set; }

        public static Expression<Func<Data.Request, Request>> Map = r => r == null ? null : new Request
        {
            Id = r.Id,
            Uri = r.Uri,
            IpAddress = r.IpAddress,
            Method = r.Method,
            OccurredOn = r.OccurredOn,
            CompletedOn = r.CompletedOn,
            StatusCode = r.StatusCode,
            StatusMessage = r.StatusMessage,
            SerializedRequestHeaders = r.RequestHeaders,
            SerializedResponseHeaders = r.ResponseHeaders,
            RequestLength = r.RequestLength,
            ResponseLength = r.ResponseLength
        };
    }
}