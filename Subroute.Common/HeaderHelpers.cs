using System;
using System.Collections.Generic;
using System.Linq;
using Subroute.Common.Extensions;

namespace Subroute.Common
{
    public static class HeaderHelpers
    {
        public static string GetDefaultHeaders()
        {
            var headers = new[]
            {
                "Content-Type: application/json",
            };

            return String.Join("\n", headers);
        }

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