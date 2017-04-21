using System;
using System.Text;
using Newtonsoft.Json;

namespace Subroute.Common
{
    /// <summary>
    /// Various helper methods for working with request and response payloads.
    /// </summary>
    public static class PayloadHelpers
    {
        /// <summary>
        /// Returns a byte array containing the JSON representation of an error payload with a message and stacktrace.
        /// </summary>
        /// <param name="message">Error message contained in the response payload.</param>
        /// <param name="stackTrace">Stack trace contained in the response payload.</param>
        /// <returns>A byte array containing the JSON representation of an error pyalod.</returns>
        public static byte[] CreateErrorPayload(string message, string stackTrace)
        {
            var payload = new { message, stackTrace };
            return GetPayloadBytes(payload);
        }

        /// <summary>
        /// Return a byte array of the JSON representation of the provided object.
        /// </summary>
        /// <param name="payload">Object to be serialized as a JSON byte array.</param>
        /// <returns>Byte array of the JSON representation of the provided object.</returns>
        public static byte[] GetPayloadBytes(object payload)
        {
            var payloadJson = JsonConvert.SerializeObject(payload, Formatting.None);
            return Encoding.UTF8.GetBytes(payloadJson);
        }
    }
}