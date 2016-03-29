using System;
using System.Net;
using System.Runtime.Serialization;
using Subroute.Core.Exceptions;

namespace Subroute.Core.Exceptions
{
    public class AlreadyExistsException : Exception, IStatusCodeException
    {
        public AlreadyExistsException()
        {
        }

        public AlreadyExistsException(string message)
            : base(message)
        {
        }

        public AlreadyExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected AlreadyExistsException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.Conflict;

        public bool IsPublic => true;
    }
}