using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class UnsupportedMediaTypeException : Exception, IStatusCodeException
    {
        public UnsupportedMediaTypeException()
        {
        }

        public UnsupportedMediaTypeException(string message) : base(message)
        {
        }

        public UnsupportedMediaTypeException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UnsupportedMediaTypeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.UnsupportedMediaType;
        public bool IsPublic => true;
    }
}