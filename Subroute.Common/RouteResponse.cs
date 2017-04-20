using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Subroute.Common
{
    [Serializable]
    public class RouteResponse
    {
        public RouteResponse(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Headers = new Dictionary<string, string>();
        }

        public HttpStatusCode StatusCode { get; }

        public string StatusMessage { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public byte[] Body { get; set; }

        public static string SerializeHeaders(IDictionary<string, string> headers)
        {
            return string.Join("\n", headers.Select(h => $"{h.Key}:{h.Value}"));
        }

        public static RouteResponse NoContent
        {
            get { return new RouteResponse(HttpStatusCode.NoContent) { Body = new byte[0] }; }
        }
    }
}