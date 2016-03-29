using System;
using System.Collections.Specialized;
using System.Linq;

namespace Subroute.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static ILookup<string, string> ToLookup(this NameValueCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var pairs =
                from key in collection.Cast<string>()
                from value in collection.GetValues(key)
                select new { key, value };

            return pairs.ToLookup(pair => pair.key, pair => pair.value);
        }
    }
}