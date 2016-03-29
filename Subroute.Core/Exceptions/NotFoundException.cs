using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    public class NotFoundException : Exception, IStatusCodeException
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode StatusCode
        {
            get { return HttpStatusCode.NotFound; }
        }

        public bool IsPublic
        {
            get { return true; }
        }
    }
}