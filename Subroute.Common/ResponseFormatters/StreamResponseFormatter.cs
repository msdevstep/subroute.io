using System;
using System.IO;
using Subroute.Common.Extensions;

namespace Subroute.Common.ResponseFormatters
{
    public class StreamResponseFormatter : IResponseFormatter
    {
        public string Name => "stream";

        public byte[] WriteResponseBody(object payload)
        {
            if (payload == null)
                return new byte[0];

            using (var stream = (Stream) payload)
                return stream.ReadFully(-1);    // -1 = Use default buffer size.
        }
    }
}