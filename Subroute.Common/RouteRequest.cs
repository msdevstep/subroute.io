using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Subroute.Common.Extensions;

namespace Subroute.Common
{
    /// <summary>
    /// Represents all data from the current request.
    /// </summary>
    [Serializable]
    public class RouteRequest
    {
        /// <summary>
        /// Contructor for creating a new route request.
        /// </summary>
        /// <param name="uri">Uri of the current request.</param>
        /// <param name="method">Http method of the current request.</param>
        public RouteRequest(Uri uri, string method)
        {
            Uri = uri;
            Method = method;
            Headers = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the incoming URI for the current request.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Gets the HTTP method of the current request.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Gets the IP address of the current request.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets the request headers of the current request.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets the raw bytes of the current request.
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// Gets the query string attributes of the current request.
        /// </summary>
        public IDictionary<string, QueryStringValue> QueryString => QueryStringParser.ParseQueryString(Uri.Query);

        /// <summary>
        /// Parses the request body and deserializes the JSON request body as the specified type.
        /// </summary>
        /// <typeparam name="TPayload">Instance of TPayload to be instantiated from the request body.</typeparam>
        /// <returns>An instance of TPayload populated with data from the current request.</returns>
        public TPayload ReadBodyAsJson<TPayload>()
        {
            return ReadBodyWithFormatter<TPayload>("json");
        }

        /// <summary>
        /// Parses the request body and deserializes the XML request body as the specified type.
        /// </summary>
        /// <typeparam name="TPayload">Instance of TPayload to be instantiated from the request body.</typeparam>
        /// <returns>An instance of TPayload populated with data from the current request.</returns>
        public TPayload ReadBodyAsXml<TPayload>()
        {
            return ReadBodyWithFormatter<TPayload>("xml");
        }

        /// <summary>
        /// Parses the request body and deserializes the raw text request body as a string.
        /// </summary>
        /// <returns>String instance populated with data from the current request.</returns>
        public string ReadBodyAsString()
        {
            return ReadBodyWithFormatter<string>("string");
        }

        /// <summary>
        /// Parses the request body and returns a stream containing the raw request data.
        /// </summary>
        /// <returns>Stream instance containing data from the current request.</returns>
        public Stream ReadBodyAsStream()
        {
            return ReadBodyWithFormatter<Stream>("stream");
        }

        /// <summary>
        /// Parses multipart form data from the request body into a dictionary.
        /// </summary>
        /// <returns>Dictionary containing multipart form data from the request body.</returns>
        public IDictionary<string, QueryStringValue> ReadBodyAsPostData()
        {
            return ReadBodyWithFormatter<IDictionary<string, QueryStringValue>>("post");
        }

        /// <summary>
        /// Parses the request body and deserializes the request body as the specified type using the specified formatter.
        /// </summary>
        /// <typeparam name="TPayload">Instance of TPayload to be instantiated from the request body.</typeparam>
        /// <param name="formatterName">Name of the formatter to be used to parse the current request body.</param>
        /// <returns>An instance of TPayload populated with data from the current request using the specified formatter.</returns>
        public TPayload ReadBodyWithFormatter<TPayload>(string formatterName)
        {
            var formatter = GlobalConfiguration.RequestFormatters.FirstOrDefault(f => f.Name.CaseInsensitiveEqual(formatterName));

            if (formatter == null)
                throw new NullReferenceException($"No request formatter exists named '{formatterName}'.");

            return (TPayload) formatter.ReadRequestBody(typeof (TPayload), Body);
        }
    }
}