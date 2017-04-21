using System;

namespace Subroute.Common.RequestFormatters
{
    /// <summary>
    /// Interface for building a new request formatter.
    /// </summary>
    public interface IRequestFormatter
    {
        /// <summary>
        /// Gets the name of the current formatter, used during formatter lookup.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Deserializes the request body byte array into a concrete object of the specified type.
        /// </summary>
        /// <param name="bodyType">Type of concrete instance to create.</param>
        /// <param name="body">Byte array representing the request body.</param>
        /// <returns>Concrete object of the specified type.</returns>
        object ReadRequestBody(Type bodyType, byte[] body);
    }
}