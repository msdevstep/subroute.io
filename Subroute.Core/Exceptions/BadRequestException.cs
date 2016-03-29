using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    public class BadRequestException : Exception, IStatusCodeException
    {
        public BadRequestException()
        {
        }

        public BadRequestException(string message)
            : base(message)
        {
        }

        public BadRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BadRequestException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode StatusCode
        {
            get { return HttpStatusCode.BadRequest; }
        }

        public bool IsPublic
        {
            get { return true; }
        }
    }
}