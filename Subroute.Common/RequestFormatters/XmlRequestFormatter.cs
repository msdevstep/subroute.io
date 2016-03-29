using System;
using System.IO;
using System.Runtime.Serialization;

namespace Subroute.Common.RequestFormatters
{
    public class XmlRequestFormatter : IRequestFormatter
    {
        public string Name => "xml";

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