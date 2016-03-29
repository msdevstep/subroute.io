using System;
using System.Runtime.Serialization;

namespace Subroute.Core.Exceptions
{
    [Serializable]
    public class ServiceBusException : Exception
    {
        public ServiceBusException()
        {
        }

        public ServiceBusException(string message) : base(message)
        {
        }

        public ServiceBusException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ServiceBusException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {

        }
    }
}