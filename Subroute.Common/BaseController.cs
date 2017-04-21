using System;
using System.IO;
using System.Linq;
using System.Net;
using Subroute.Common.Extensions;

namespace Subroute.Common
{
    /// <summary>
    /// Base class containing useful methods for working with a request.
    /// </summary>
    public abstract class BaseController
    {
        /// <summary>
        /// Protected constructor for creating an instance of <see cref="BaseController"/>
        /// </summary>
        protected BaseController()
        {

        }

        /// <summary>
        /// Returns an empty response with the specified status code.
        /// </summary>
        /// <param name="statusCode">Status code to be returned for the current request.</param>
        /// <returns>Empty <see cref="RouteResponse"/> object containing the specified status code.</returns>
        public RouteResponse StatusCode(HttpStatusCode statusCode)
        {
            return new RouteResponse(statusCode)
            {
                Body = new byte[0]
            };
        }

        /// <summary>
        /// Returns an empty response with a 400 Bad Request status code.
        /// </summary>
        /// <returns>Empty response object with a 400 Bad Request status code.</returns>
        public RouteResponse BadRequest()
        {
            return StatusCode(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Returns an empty response with a 204 No Content status code.
        /// </summary>
        /// <returns>Empty response object with a 204 No Content status code.</returns>
        public RouteResponse NoContent()
        {
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Returns a response object with the specified status code containing the JSON serialized data of the specified payload object.
        /// </summary>
        /// <param name="statusCode">Status code to be returned for the current request.</param>
        /// <param name="payload">Instance of an object to be serialized as JSON.</param>
        /// <returns><see cref="RouteResponse"/> instance containing the JSON serialized payload.</returns>
        public RouteResponse Json(HttpStatusCode statusCode, object payload)
        {
            var response = Formatted(statusCode, "json", payload);
            response.Headers.Add("Content-Type", "application/json");
            return response;
        }

        /// <summary>
        /// Returns a response object with the specified status code containing the XML serialized data of the specified payload object.
        /// </summary>
        /// <param name="statusCode">Status code to be returned for the current request.</param>
        /// <param name="payload">Instance of an object to be serialized as XML.</param>
        /// <returns><see cref="RouteResponse"/> instance containing the XML serialized payload.</returns>
        public RouteResponse Xml(HttpStatusCode statusCode, object payload)
        {
            var response = Formatted(statusCode, "xml", payload);
            response.Headers.Add("Content-Type", "application/xml");
            return response;
        }

        /// <summary>
        /// Returns a response object with the specified status code containing the specified string data.
        /// </summary>
        /// <param name="statusCode">Status code to be returned for the current request.</param>
        /// <param name="payload">String representation of the response payload.</param>
        /// <returns><see cref="RouteResponse"/> instance containing the specified string data.</returns>
        public RouteResponse String(HttpStatusCode statusCode, string payload)
        {
            var response = Formatted(statusCode, "string", payload);
            response.Headers.Add("Content-Type", "text/plain");
            return response;
        }

        /// <summary>
        /// Returns a response object with the specified status code containing the provided stream data.
        /// </summary>
        /// <param name="statusCode">Status code to be returned for the current request.</param>
        /// <param name="payload">Stream data to be returned with the response payload.</param>
        /// <param name="contentType">Content type header value to be returned with the response.</param>
        /// <param name="contentDisposition">Content disposition header to be returned with the response.</param>
        /// <returns><see cref="RouteResponse"/> instance containing the specified headers and data.</returns>
        public RouteResponse Stream(HttpStatusCode statusCode, Stream payload, string contentType, string contentDisposition = null)
        {
            var response = Formatted(statusCode, "stream", payload);
            response.Headers.Add("Content-Type", contentType);

            if (contentDisposition != null)
            {
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Disposition");
                response.Headers.Add("Content-Disposition", contentDisposition);
            }

            return response;
        }

        /// <summary>
        /// Returns a response object with the specified payload serialized with the specified formatter.
        /// </summary>
        /// <param name="statusCode">Status code to be returned for the current request.</param>
        /// <param name="formatterName">Name of the formatter to be used to format the request body.</param>
        /// <param name="payload">Instance to be provided to the formatter for serialization.</param>
        /// <returns><see cref="RouteResponse"/> instance containing the formatted payload.</returns>
        public RouteResponse Formatted(HttpStatusCode statusCode, string formatterName, object payload)
        {
            var formatter = GlobalConfiguration.ResponseFormatters.FirstOrDefault(f => f.Name.CaseInsensitiveEqual(formatterName));

            if (formatter == null)
                throw new NullReferenceException($"No response formatter exists named '{formatterName}'.");

            return new RouteResponse(statusCode)
            {
                Body = formatter.WriteResponseBody(payload)
            };
        }
    }
}