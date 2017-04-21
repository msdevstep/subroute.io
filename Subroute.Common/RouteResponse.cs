using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Subroute.Common
{
    /// <summary>
    /// Represents all data for outgoing response.
    /// </summary>
    [Serializable]
    public class RouteResponse
    {
        /// <summary>
        /// Constructor for creating new route response with specific status code.
        /// </summary>
        /// <param name="statusCode">Status code of the outgoing response.</param>
        public RouteResponse(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the status code of the outgoing response.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets or sets the status message of the outgoing response.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the headers of the outgoing response.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets or sets the raw byte array containing the body of the outgoing response.
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// Serializes the specified header dictionary as a string.
        /// </summary>
        /// <param name="headers">Dictionary containing headers to be serialized.</param>
        /// <returns>String representation of header data.</returns>
        public static string SerializeHeaders(IDictionary<string, string> headers)
        {
            return string.Join("\n", headers.Select(h => $"{h.Key}:{h.Value}"));
        }

        /// <summary>
        /// Static method for creating an empty 204 No Content response.
        /// </summary>
        public static RouteResponse NoContent
        {
            get { return new RouteResponse(HttpStatusCode.NoContent) { Body = new byte[0] }; }
        }
    }
}