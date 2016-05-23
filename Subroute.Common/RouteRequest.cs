using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Subroute.Common.Extensions;

namespace Subroute.Common
{
    [Serializable]
    public class RouteRequest
    {
        public RouteRequest(Uri uri, string method)
        {
            Uri = uri;
            Method = method;
            Headers = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public Uri Uri { get; }

        public string Method { get; }

        public string IpAddress { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public byte[] Body { get; set; }

        public TPayload ReadBodyAsJson<TPayload>()
        {
            return ReadBodyWithFormatter<TPayload>("json");
        }

        public TPayload ReadBodyAsXml<TPayload>()
        {
            return ReadBodyWithFormatter<TPayload>("xml");
        }

        public string ReadBodyAsString()
        {
            return ReadBodyWithFormatter<string>("string");
        }

        public Stream ReadBodyAsStream()
        {
            return ReadBodyWithFormatter<Stream>("stream");
        }

        public IDictionary<string, QueryStringValue> ReadBodyAsPostData()
        {
            return ReadBodyWithFormatter<IDictionary<string, QueryStringValue>>("post");
        }

        public TPayload ReadBodyWithFormatter<TPayload>(string formatterName)
        {
            var formatter = GlobalConfiguration.RequestFormatters.FirstOrDefault(f => f.Name.CaseInsensitiveEqual(formatterName));

            if (formatter == null)
                throw new NullReferenceException($"No request formatter exists named '{formatterName}'.");

            return (TPayload) formatter.ReadRequestBody(typeof (TPayload), Body);
        }
    }

    public class QueryStringValue
    {
        public QueryStringValue(string[] values)
        {
            Values = values ?? new string[0];
            FirstValue = Values.FirstOrDefault();
        }

        public string FirstValue { get; }
        public string[] Values { get; }

        public static implicit operator string(QueryStringValue value)
        {
            return value.FirstValue;
        }
    }
}