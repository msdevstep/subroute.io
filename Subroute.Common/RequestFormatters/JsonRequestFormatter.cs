using System;
using System.Text;
using Newtonsoft.Json;

namespace Subroute.Common.RequestFormatters
{
    public class JsonRequestFormatter : IRequestFormatter
    {
        public string Name => "json";

        public object ReadRequestBody(Type bodyType, byte[] body)
        {
            var json = Encoding.UTF8.GetString(body ?? new byte[0]);
            return JsonConvert.DeserializeObject(json, bodyType);
        }
    }
}