using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Subroute.Core.Utilities;

namespace Subroute.Api.Models.Requests
{
    public class RequestResponse
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public string Method { get; set; }
        public string Uri { get; set; }
        public int? StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public IDictionary<string, string> RequestHeaders { get; set; }
        public byte[] RequestPayload { get; set; }
        public IDictionary<string, string> ResponseHeaders { get; set; }
        public byte[] ResponsePayload { get; set; }
        public long RequestLength { get; set; }
        public long? ResponseLength { get; set; }
        public string IpAddress { get; set; }
        public DateTimeOffset OccurredOn { get; set; }
        public DateTimeOffset? CompletedOn { get; set; }

        public static Func<Core.Data.Request, RequestResponse> Map = r => r == null ? null : new RequestResponse
        {
            Id = r.Id,
            Uri = r.Uri,
            RouteId = r.RouteId,
            IpAddress = r.IpAddress,
            Method = r.Method,
            OccurredOn = r.OccurredOn,
            CompletedOn = r.CompletedOn,
            StatusCode = r.StatusCode,
            StatusMessage = r.StatusMessage,
            RequestHeaders = HeaderUtility.DeserializeHeaders(r.RequestHeaders),
            ResponseHeaders = HeaderUtility.DeserializeHeaders(r.ResponseHeaders),
            RequestLength = r.RequestLength,
            ResponseLength = r.ResponseLength,
            RequestPayload = r.RequestPayload,
            ResponsePayload = r.ResponsePayload
        };
    }
}
