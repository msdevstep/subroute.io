using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class RouteException : Exception, IStatusCodeException
    {
        public RouteException()
        {
        }

        public RouteException(string message) : base(message)
        {
        }

        public RouteException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RouteException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
        public bool IsPublic => true;
    }
}