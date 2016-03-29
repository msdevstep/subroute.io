using System;
using System.Runtime.Serialization;

namespace Subroute.Common
{
    [Serializable]
    public class EntryPointException : Exception
    {
        public EntryPointException()
        {
        }

        public EntryPointException(string message) : base(message)
        {
        }

        public EntryPointException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EntryPointException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}