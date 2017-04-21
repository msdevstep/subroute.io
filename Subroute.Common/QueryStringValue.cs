using System.Linq;

namespace Subroute.Common
{
    /// <summary>
    /// Contains data for a query string key.
    /// </summary>
    public class QueryStringValue
    {
        /// <summary>
        /// Creates a new instance of QueryStringValue.
        /// </summary>
        /// <param name="values">Values represented by a single query string key.</param>
        public QueryStringValue(string[] values)
        {
            Values = values ?? new string[0];
            FirstValue = Values.FirstOrDefault();
        }

        /// <summary>
        /// Gets the first value for a query string key.
        /// </summary>
        public string FirstValue { get; }

        /// <summary>
        /// Gets all the values for a query string key.
        /// </summary>
        public string[] Values { get; }

        /// <summary>
        /// Implicitly returns the first value for a query string key.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string(QueryStringValue value)
        {
            return value.FirstValue;
        }
    }
}