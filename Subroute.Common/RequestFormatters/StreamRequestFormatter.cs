using System;
using System.IO;

namespace Subroute.Common.RequestFormatters
{
    public class StreamRequestFormatter : IRequestFormatter
    {
        public string Name => "stream";

        public object ReadRequestBody(Type bodyType, byte[] body)
        {
            var stream = new MemoryStream(body);
            stream.Position = 0;
            return stream;
        }
    }
}