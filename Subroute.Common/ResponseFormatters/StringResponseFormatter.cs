using System;
using System.Text;

namespace Subroute.Common.ResponseFormatters
{
    public class StringResponseFormatter : IResponseFormatter
    {
        public string Name => "string";

        public byte[] WriteResponseBody(object payload)
        {
            var stringOutput = payload?.ToString() ?? string.Empty;
            return Encoding.UTF8.GetBytes(stringOutput);
        }
    }
}