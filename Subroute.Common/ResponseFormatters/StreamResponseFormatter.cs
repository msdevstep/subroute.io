using System;
using System.IO;
using Subroute.Common.Extensions;

namespace Subroute.Common.ResponseFormatters
{
    /// <summary>
    /// Formats the response payload as a stream.
    /// </summary>
    public class StreamResponseFormatter : IResponseFormatter
    {
        /// <summary>
        /// Gets the name of the current StreamResponseFormatter (stream).
        /// </summary>
        public string Name => "stream";

        /// <summary>
        /// Serializes the provided payload object into a byte array.
        /// </summary>
        /// <param name="payload">Instance to be serialized as a stream.</param>
        /// <returns>Byte array representing the provided payload as a stream.</returns>
        public byte[] WriteResponseBody(object payload)
        {
            if (payload == null)
                return new byte[0];

            using (var stream = (Stream) payload)
                return stream.ReadFully(-1);    // -1 = Use default buffer size.
        }
    }
}