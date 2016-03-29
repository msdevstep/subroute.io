using System;
using System.Text;
using Newtonsoft.Json;

namespace Subroute.Common
{
    public static class PayloadHelpers
    {
        public static byte[] CreateErrorPayload(string message, string stackTrace)
        {
            var payload = new { message, stackTrace };
            return GetPayloadBytes(payload);
        }

        public static byte[] GetPayloadBytes(object payload)
        {
            var payloadJson = JsonConvert.SerializeObject(payload, Formatting.None);
            return Encoding.UTF8.GetBytes(payloadJson);
        }
    }
}