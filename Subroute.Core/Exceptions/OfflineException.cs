using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class OfflineException : Exception, IStatusCodeException
    {
        public OfflineException()
        {
        }

        public OfflineException(string message) : base(message)
        {
        }

        public OfflineException(string message, Exception inner) : base(message, inner)
        {
        }

        protected OfflineException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public HttpStatusCode StatusCode
        {
            get { return HttpStatusCode.ServiceUnavailable; }
        }

        public bool IsPublic
        {
            get { return true; }
        }
    }
}