using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class QueryException : Exception, IStatusCodeException
    {
        public QueryException()
        {
        }

        public QueryException(string message) : base(message)
        {
        }

        public QueryException(string message, Exception inner) : base(message, inner)
        {
        }

        protected QueryException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
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