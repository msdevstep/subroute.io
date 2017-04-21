using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;

namespace Subroute.Common.RequestFormatters
{
    /// <summary>
    /// Formats the JSON request body as the specified type.
    /// </summary>
    public class JsonRequestFormatter : IRequestFormatter
    {
        /// <summary>
        /// Gets the name of the current JsonRequestFormatter (json).
        /// </summary>
        public string Name => "json";

        /// <summary>
        /// Deserializes the request body as the specified concrete type.
        /// </summary>
        /// <param name="bodyType">Type of concrete object to be created.</param>
        /// <param name="body">Byte array containing request body.</param>
        /// <returns>Concrete instance of specified type.</returns>
        public object ReadRequestBody(Type bodyType, byte[] body)
        {
            var json = Encoding.UTF8.GetString(body ?? new byte[0]);
            return JsonConvert.DeserializeObject(json, bodyType);
        }
    }
}