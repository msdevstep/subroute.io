using System;
using System.IO;
using System.Runtime.Serialization;
using Subroute.Common.Extensions;

namespace Subroute.Common.ResponseFormatters
{
    public class XmlResponseFormatter : IResponseFormatter
    {
        public string Name => "xml";

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