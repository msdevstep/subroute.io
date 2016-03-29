using System;
using System.IO;
using System.Linq;
using System.Net;
using Subroute.Common.Extensions;

namespace Subroute.Common
{
    public abstract class BaseController
    {
        protected BaseController()
        {

        }

        public RouteResponse NoContent()
        {
            return new RouteResponse(HttpStatusCode.NoContent)
            {
                Body = new byte[0]
            };
        }

        public RouteResponse Json(HttpStatusCode statusCode, object payload)
        {
            var response = Formatted(statusCode, "json", payload);
            response.Headers.Add("Content-Type", "application/json");
            return response;
        }

        public RouteResponse Xml(HttpStatusCode statusCode, object payload)
        {
            var response = Formatted(statusCode, "xml", payload);
            response.Headers.Add("Content-Type", "application/xml");
            return response;
        }

        public RouteResponse String(HttpStatusCode statusCode, string payload)
        {
            var response = Formatted(statusCode, "string", payload);
            response.Headers.Add("Content-Type", "text/plain");
            return response;
        }

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