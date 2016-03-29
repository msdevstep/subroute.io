using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class RoutePermissionException : Exception, IStatusCodeException
    {
        public RoutePermissionException()
        {
        }

        public RoutePermissionException(string message) : base(message)
        {
        }

        public RoutePermissionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RoutePermissionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
        public bool IsPublic => true;
    }
}