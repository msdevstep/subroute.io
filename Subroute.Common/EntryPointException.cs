using System;
using System.Runtime.Serialization;

namespace Subroute.Common
{
    /// <summary>
    /// Exception indicating a problem locating a method to be executed for the current request.
    /// </summary>
    [Serializable]
    public class EntryPointException : Exception
    {
        /// <summary>
        /// Default constructor for <see cref="EntryPointException"/>.
        /// </summary>
        public EntryPointException()
        {
        }

        /// <summary>
        /// Constructor accepting a message for the current exception.
        /// </summary>
        /// <param name="message">String containing the current exception method.</param>
        public EntryPointException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor accepting a message and inner exception for the current exception.
        /// </summary>
        /// <param name="message">String containing the current exception method.</param>
        /// <param name="inner">Inner exception for the current exception.</param>
        public EntryPointException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Constructor accepting data for serializaing the exception.
        /// </summary>
        /// <param name="info">Serialization info for the current exception.</param>
        /// <param name="context">Streaming context for the current exception.</param>
        protected EntryPointException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}