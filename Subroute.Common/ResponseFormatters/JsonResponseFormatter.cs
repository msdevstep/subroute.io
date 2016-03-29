using System;
using System.Text;
using Newtonsoft.Json;

namespace Subroute.Common.ResponseFormatters
{
    public class JsonResponseFormatter : IResponseFormatter
    {
        public string Name => "json";

        public byte[] WriteResponseBody(object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}