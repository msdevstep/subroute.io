using System;

namespace Subroute.Common.ResponseFormatters
{
    /// <summary>
    /// Interface for building a new reponse formatter.
    /// </summary>
    public interface IResponseFormatter
    {
        /// <summary>
        /// Gets the name of the current formatter, used during formatter lookup.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Converts an instance of any object to a byte array representing the response body.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        byte[] WriteResponseBody(object payload);
    }
}