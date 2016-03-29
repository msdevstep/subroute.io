using System;
using System.ComponentModel.DataAnnotations;

namespace Subroute.Core.Data
{
    public class Request
    {
        [Key]
        public int Id { get; set; }
        public int RouteId { get; set; }
        public string Method { get; set; }
        public string Uri { get; set; }
        public int? StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string RequestHeaders { get; set; }
        public byte[] RequestPayload { get; set; }
        public string ResponseHeaders { get; set; }
        public byte[] ResponsePayload { get; set; }
        public long RequestLength { get; set; }
        public long? ResponseLength { get; set; }
        public string IpAddress { get; set; }
        public DateTimeOffset OccurredOn { get; set; }
        public DateTimeOffset? CompletedOn { get; set; }

        public virtual Route Route { get; set; }
    }
}