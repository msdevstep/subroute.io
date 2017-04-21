using System;
using System.IO;
using System.Runtime.Serialization;

namespace Subroute.Common.RequestFormatters
{
    /// <summary>
    /// Formats the XML request body as the specified type.
    /// </summary>
    public class XmlRequestFormatter : IRequestFormatter
    {
        /// <summary>
        /// Gets the name of the current XmlRequestFormatter (xml).
        /// </summary>
        public string Name => "xml";

        /// <summary>
        /// Deserializes the request body as the specified concrete type.
        /// </summary>
        /// <param name="bodyType">Type of concrete object to be created.</param>
        /// <param name="body">Byte array containing request body.</param>
        /// <returns>Concrete instance of specified type.</returns>
        public object ReadRequestBody(Type bodyType, byte[] body)
        {
            using (var stream = new MemoryStream(body))
            {
                var serializer = new DataContractSerializer(bodyType, new DataContractSerializerSettings() { });
                return serializer.ReadObject(stream);
            }
        }
    }
}