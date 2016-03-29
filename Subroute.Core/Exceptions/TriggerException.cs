using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class CodeException : Exception, IStatusCodeException
    {
        public CodeException()
        {
        }

        public CodeException(string message)
            : base(message)
        {
        }

        public CodeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CodeException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
        public bool IsPublic => true;
    }
}