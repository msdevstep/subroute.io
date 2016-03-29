using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    public class AuthenticationException : Exception, IStatusCodeException
    {
        public AuthenticationException()
        {
        }

        public AuthenticationException(string message)
            : base(message)
        {
        }

        public AuthenticationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected AuthenticationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;

        public bool IsPublic => true;
    }
}