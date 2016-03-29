using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class RouteEntryPointException : Exception, IStatusCodeException
    {
        public RouteEntryPointException()
        {
        }

        public RouteEntryPointException(string message) : base(message)
        {
        }

        public RouteEntryPointException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RouteEntryPointException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.NotImplemented;
        public bool IsPublic => true;
    }
}