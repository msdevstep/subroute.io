using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class ValidationException : Exception, IStatusCodeException
    {
        public ValidationException()
        {
        }

        public ValidationException(ValidationResult validationResult)
        {
            ValidationResult = validationResult;
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ValidationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public ValidationResult ValidationResult { get; private set; }

        public HttpStatusCode StatusCode
        {
            get { return HttpStatusCode.BadRequest; }
        }

        public bool IsPublic
        {
            get { return true;  }
        }
    }
}