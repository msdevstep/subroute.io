using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subroute.Core.Utilities
{
    public static class HeaderUtility
    {
        public static string SerializeHeaders(IDictionary<string, IEnumerable<string>> headers)
        {
            return string.Join("\n", headers.Select(h => $"{h.Key}:{string.Join(", ", h.Value)}"));
        }

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
