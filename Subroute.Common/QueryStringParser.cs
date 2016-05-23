using System;
using System.Collections.Generic;
using System.Linq;

namespace Subroute.Common
{
    public static class QueryStringParser
    {
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

                    // However, if we don't have exactly two segments, then we have
                    // no name and value. We should still get two segments even if
                    // the value is empty.
                    if (segments.Length != 2)
                        return null;

                    // We'll pass it back as a tuple to keep the name and value seperate
                    // without having to create a new concrete type (since this is internal).
                    // We'll unescape the value since it may be escaped for wire transfer.
                    return new Tuple<string, string>(segments[0], Uri.UnescapeDataString(segments[1]));
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