using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class ConflictException : Exception, IStatusCodeException
    {
        public ConflictException()
        {
        }

        public ConflictException(string message)
            : base(message)
        {
        }

        public ConflictException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ConflictException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode StatusCode { get { return HttpStatusCode.Conflict; } }

        public bool IsPublic
        {
            get { return true; }
        }
    }
}