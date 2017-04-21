using System;
using System.Text;

namespace Subroute.Common.RequestFormatters
{
    /// <summary>
    /// Formats the string request body as the specified type.
    /// </summary>
    public class StringRequestFormatter : IRequestFormatter
    {
        /// <summary>
        /// Gets the name of the current StringRequestFormatter (string).
        /// </summary>
        public string Name => "string";

        /// <summary>
        /// Deserializes the request body as the specified concrete type.
        /// </summary>
        /// <param name="bodyType">Type of concrete object to be created.</param>
        /// <param name="body">Byte array containing request body.</param>
        /// <returns>Concrete instance of specified type.</returns>
        public object ReadRequestBody(Type bodyType, byte[] body)
        {
            return Encoding.UTF8.GetString(body ?? new byte[0]);
        }
    }
}