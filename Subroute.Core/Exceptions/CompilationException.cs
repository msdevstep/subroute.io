using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class CompilationException : Exception, IStatusCodeException
    {
        public CompilationException()
        {
        }

        public CompilationException(string message)
            : base(message)
        {
        }

        public CompilationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CompilationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode StatusCode
        {
            get { return HttpStatusCode.InternalServerError; }
        }

        public bool IsPublic
        {
            get { return true; }
        }
    }
}