using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Subroute.Core.Execution
{
    public class ExecutionResponse
    {
        public ExecutionResponse(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Headers = new Dictionary<string, string>();
        }

        public HttpStatusCode StatusCode { get; }

        public string StatusMessage { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public byte[] Body { get; set; }

        public static IDictionary<string, string> DeserializeHeaders(string headers)
        {
            var result = new Dictionary<string, string>();

            // Iterate over each line in the string as one line is equal to one header.
            foreach (var line in (headers ?? string.Empty).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // We have to seperate the key from the values by splitting on semi-colon.
                var segments = line.Split(':');

                // Ignore malformed headers.
                if (segments.Length != 2)
                    continue;

                result.Add(segments[0], segments[1]);
            }

            return result;
        }
    }
}