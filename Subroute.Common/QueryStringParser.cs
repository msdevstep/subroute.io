using System;
using System.Collections.Generic;
using System.Linq;

namespace Subroute.Common
{
    /// <summary>
    /// Helper methods for working with query string data.
    /// </summary>
    public static class QueryStringParser
    {
        /// <summary>
        /// Parses a string containing query string formatted data.
        /// </summary>
        /// <param name="query">String containing query string formatted data.</param>
        /// <returns>Dictionary containing the provided query string data.</returns>
        public static IDictionary<string, QueryStringValue> ParseQueryString(string query)
        {
            // Remove any leading question mark for the case of query string parsing.
            query = query.TrimStart('?');

            // We'll split the query string on ampersand and remove any blank entries
            // as they are no use to us, and should only occur from doubled up ampersands.
            return query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p =>
                {
                    // We'll then split each query string variable on equal to seperate
                    // its name from its value. We don't want to remove empty entries
                    // because an empty entry is a valid value.
                    var segments = p.Split('=');

                    // Having more than two segments should result in a bad request. This
                    // would indicate a value such as waffle=cone=pancake, which isn't valid
                    // unless it has been escaped, and escaped values will be handled correctly.
                    if (segments.Length > 2)
                        throw new BadRequestException("Query string is improperly formatted, please verify query format.");

                    // However, if we didn't receive any segments, then we have an invalid
                    // value and we'll return null so that it's filtered out.
                    if (segments.Length == 0)
                        return null;

                    // We need to try and get and escape the value if we can. Some REST
                    // clients will automatically remove the equal sign if no value is
                    // present, in that case, we'll just have the query string variables name.
                    var value = segments.Length > 1 ? Uri.UnescapeDataString(segments[1]) : null;

                    // We'll pass it back as a tuple to keep the name and value seperate
                    // without having to create a new concrete type (since this is internal).
                    // We'll unescape the value since it may be escaped for wire transfer.
                    return new Tuple<string, string>(segments[0], value);
                })
                // We'll ignore null variables since they were variables that didn't
                // contain a name and a value. Then we'll group by variable name
                // since the same variable can be passed as many times as the developer
                // likes. We'll surface all available values using the QueryStringValue
                // type, this type make it easy to deal with single values as well as
                // multi-value variables.
                .Where(p => p != null)
                .GroupBy(p => p.Item1)
                .ToDictionary(g => g.Key, g => new QueryStringValue(g.Select(p => p.Item2).ToArray()));
        }
    }
}