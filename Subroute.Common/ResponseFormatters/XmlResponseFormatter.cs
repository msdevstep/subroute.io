using System;
using System.IO;
using System.Runtime.Serialization;
using Subroute.Common.Extensions;

namespace Subroute.Common.ResponseFormatters
{
    /// <summary>
    /// Formats the response payload as XML.
    /// </summary>
    public class XmlResponseFormatter : IResponseFormatter
    {
        /// <summary>
        /// Gets the name of the current XmlResponseFormatter (xml).
        /// </summary>
        public string Name => "xml";

        /// <summary>
        /// Serializes the provided payload object into an XML byte array.
        /// </summary>
        /// <param name="payload">Instance to be serialized as XML.</param>
        /// <returns>Byte array representing the provided payload as XML.</returns>
        public byte[] WriteResponseBody(object payload)
        {
            if (payload == null)
                return new byte[0];

            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(payload.GetType());
                serializer.WriteObject(stream, payload);
                stream.Position = 0;

                return stream.ReadFully(-1); // -1 = Use default buffer size.
            }
        }
    }
}