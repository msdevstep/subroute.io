using System;
using System.Text;
using Newtonsoft.Json;

namespace Subroute.Common.ResponseFormatters
{
    /// <summary>
    /// Formats the response payload as JSON.
    /// </summary>
    public class JsonResponseFormatter : IResponseFormatter
    {
        /// <summary>
        /// Gets the name of the current JsonResponseFormatter (json).
        /// </summary>
        public string Name => "json";

        /// <summary>
        /// Serializes the provided payload object into a JSON byte array.
        /// </summary>
        /// <param name="payload">Instance to be serialized as JSON.</param>
        /// <returns>Byte array representing the provided payload as JSON.</returns>
        public byte[] WriteResponseBody(object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}