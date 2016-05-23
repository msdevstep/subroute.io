using System;
using System.Text;

namespace Subroute.Common.RequestFormatters
{
    public class PostRequestFormatter : IRequestFormatter
    {
        public string Name => "post";

        public object ReadRequestBody(Type bodyType, byte[] body)
        {
            // Extract text only from request body.
            var text = Encoding.UTF8.GetString(body ?? new byte[0]);

            return QueryStringParser.ParseQueryString(text);
        }
    }
}