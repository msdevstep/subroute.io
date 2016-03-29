using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Subroute.Core.Execution
{
    public class ExecutionRequest
    {
        public ExecutionRequest(Uri uri, HttpMethod method)
        {
            Uri = uri;
            Method = method;
        }

        public Uri Uri { get; }

        public HttpMethod Method { get; }

        public string IpAddress { get; set; }

        public IDictionary<string, IEnumerable<string>> Headers { get; set; }

        public byte[] Body { get; set; }
    }
}