using System;
using System.Text;

namespace Subroute.Common.RequestFormatters
{
    public class StringRequestFormatter : IRequestFormatter
    {
        public string Name => "string";

        public object ReadRequestBody(Type bodyType, byte[] body)
        {
            return Encoding.UTF8.GetString(body ?? new byte[0]);
        }
    }
}