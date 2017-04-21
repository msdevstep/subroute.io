using System;
using System.Net;
using System.Runtime.Serialization;

namespace Subroute.Common
{
    /// <summary>
    /// Exception indicating an issue with the current request.
    /// </summary>
    [Serializable]
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Default constructor for <see cref="BadRequestException"/>.
        /// </summary>
        public BadRequestException()
        {
        }

        /// <summary>
        /// Constructor accepting a message for the current exception.
        /// </summary>
        /// <param name="message"></param>
        public BadRequestException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor accepting a message and inner exception for the current exception.
        /// </summary>
        /// <param name="message">String containing the current exception method.</param>
        /// <param name="inner">Inner exception for the current exception.</param>
        public BadRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Constructor accepting data for serializaing the exception.
        /// </summary>
        /// <param name="info">Serialization info for the current exception.</param>
        /// <param name="context">Streaming context for the current exception.</param>
        protected BadRequestException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}