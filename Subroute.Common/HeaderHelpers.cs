using System;
using System.Collections.Generic;
using System.Linq;
using Subroute.Common.Extensions;

namespace Subroute.Common
{
    /// <summary>
    /// Class containing various methods for working with headers.
    /// </summary>
    public static class HeaderHelpers
    {
        /// <summary>
        /// Returns the default response headers.
        /// </summary>
        /// <returns>String containing the default response headers.</returns>
        public static string GetDefaultHeaders()
        {
            var headers = new[]
            {
                "Content-Type: application/json",
            };

            return String.Join("\n", headers);
        }

        /// <summary>
        /// Deserializes the string representation of header data.
        /// </summary>
        /// <param name="headers">String containing the header data.</param>
        /// <returns>Dictionary containing the parsed header data.</returns>
        public static IDictionary<string, string> DeserializeHeaders(string headers)
        {
            var result = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

            // Iterate over each line in the string as one line is equal to one header.
            foreach (var line in (headers ?? String.Empty).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
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

        /// <summary>
        /// Returns whether the provided content type exist in the header dictionary.
        /// </summary>
        /// <param name="headers">Dictionary containing header data.</param>
        /// <param name="contentTypes">Content types to be located in the header data dictionary.</param>
        /// <returns>Boolean value indicated whether the content type exists in the header data.</returns>
        public static bool ContainsContentType(IDictionary<string, string> headers, params string[] contentTypes)
        {
            if (headers == null)
                return false;

            // Try to get content type header.
            foreach (var contentType in contentTypes)
            {
                string value;
                if (!headers.TryGetValue("Content-Type", out value))
                    continue;

                if (value.CaseInsensitiveContains(contentType))
                    return true;
            }
            
            return false;
        }
    }
}