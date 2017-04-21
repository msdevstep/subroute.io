using System;
using System.IO;

namespace Subroute.Common.RequestFormatters
{
    /// <summary>
    /// Formats the stream request body as the specified type.
    /// </summary>
    public class StreamRequestFormatter : IRequestFormatter
    {
        /// <summary>
        /// Gets the name of the current StreamRequestFormatter (stream).
        /// </summary>
        public string Name => "stream";

        /// <summary>
        /// Deserializes the request body as the specified concrete type.
        /// </summary>
        /// <param name="bodyType">Type of concrete object to be created.</param>
        /// <param name="body">Byte array containing request body.</param>
        /// <returns>Concrete instance of specified type.</returns>
        public object ReadRequestBody(Type bodyType, byte[] body)
        {
            var stream = new MemoryStream(body);
            stream.Position = 0;
            return stream;
        }
    }
}