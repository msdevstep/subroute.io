﻿using System;
using System.Runtime.Serialization;

namespace Subroute.Common.Exceptions
{
    /// <summary>
    /// Exception indicating an issue with the current request.
    /// </summary>
    [Serializable]
    public class SubrouteException : Exception
    {
        /// <summary>
        /// Default constructor for <see cref="SubrouteException"/>.
        /// </summary>
        public SubrouteException()
        {
        }

        /// <summary>
        /// Constructor accepting a message for the current exception.
        /// </summary>
        /// <param name="message"></param>
        public SubrouteException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor accepting a message and inner exception for the current exception.
        /// </summary>
        /// <param name="message">String containing the current exception method.</param>
        /// <param name="inner">Inner exception for the current exception.</param>
        public SubrouteException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Constructor accepting data for serializaing the exception.
        /// </summary>
        /// <param name="info">Serialization info for the current exception.</param>
        /// <param name="context">Streaming context for the current exception.</param>
        protected SubrouteException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}