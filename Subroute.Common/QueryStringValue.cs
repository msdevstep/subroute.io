using Subroute.Common.Extensions;
using System.Linq;
using System.Net;

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
            FirstValue = WebUtility.UrlDecode(Values.FirstOrDefault());
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
        /// Attempts to cast, convert, or otherwise coerce the first query string value as the specified type.
        /// </summary>
        /// <typeparam name="T">Expected result type to return the first query string value as.</typeparam>
        /// <param name="defaultValue">Value to return if the value cannot be made into the result type.</param>
        /// <returns>First query string value as the specified type.</returns>
        public T As<T>(T defaultValue = default(T))
        {
            try
            {
                return (T)typeof(T).Coerce(FirstValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Implicitly returns the first value for a query string key.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string(QueryStringValue value)
        {
            return value.FirstValue;
        }

        /// <summary>
        /// Returns the string containing QueryStringValue.FirstValue.
        /// </summary>
        /// <returns>String containing QueryStringValue.FirstValue.</returns>
        public override string ToString()
        {
            return FirstValue;
        }
    }
}