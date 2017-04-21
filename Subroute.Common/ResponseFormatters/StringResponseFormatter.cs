using System;
using System.Text;

namespace Subroute.Common.ResponseFormatters
{
    /// <summary>
    /// Formats the response payload as a string.
    /// </summary>
    public class StringResponseFormatter : IResponseFormatter
    {
        /// <summary>
        /// Gets the name of the current StringResponseFormatter (string).
        /// </summary>
        public string Name => "string";

        /// <summary>
        /// Serializes the provided payload object into a string byte array.
        /// </summary>
        /// <param name="payload">Instance to be serialized as a string.</param>
        /// <returns>Byte array representing the provided payload as a string.</returns>
        public byte[] WriteResponseBody(object payload)
        {
            var stringOutput = payload?.ToString() ?? string.Empty;
            return Encoding.UTF8.GetBytes(stringOutput);
        }
    }
}