using System;
using System.Text;

namespace Subroute.Common.RequestFormatters
{
    /// <summary>
    /// Formats the multipart form request body as the specified type.
    /// </summary>
    public class PostRequestFormatter : IRequestFormatter
    {
        /// <summary>
        /// Gets the name of the current PostRequestFormatter (post).
        /// </summary>
        public string Name => "post";

        /// <summary>
        /// Deserializes the request body as the specified concrete type.
        /// </summary>
        /// <param name="bodyType">Type of concrete object to be created.</param>
        /// <param name="body">Byte array containing request body.</param>
        /// <returns>Concrete instance of specified type.</returns>
        public object ReadRequestBody(Type bodyType, byte[] body)
        {
            // Extract text only from request body.
            var text = Encoding.UTF8.GetString(body ?? new byte[0]);

            return QueryStringParser.ParseQueryString(text);
        }
    }
}