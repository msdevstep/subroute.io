using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Subroute.Common.Extensions
{
    /// <summary>
    /// Various extension methods for working with strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Compares two string to determine if they are equal and ignores case.
        /// </summary>
        /// <param name="value1">First value to be compared.</param>
        /// <param name="value2">Second value to be compared.</param>
        /// <returns>Boolean indicating whether the two strings are equal.</returns>
        public static bool CaseInsensitiveEqual(this string value1, string value2)
        {
            return string.Equals(value1, value2, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Determines if the specified array contains the specified string and ignores case.
        /// </summary>
        /// <param name="array">Array to be searched.</param>
        /// <param name="value">Value to be located in array.</param>
        /// <returns>Boolean indicating whether the array contains the string.</returns>
        public static bool CaseInsensitiveContains(this IEnumerable<string> array, string value)
        {
            if (array == null)
                return false;

            return array.Any(a => CaseInsensitiveEqual(a, value));
        }

        /// <summary>
        /// Determines if the first value contains the second value and ignores case.
        /// </summary>
        /// <param name="value1">Value to be searched.</param>
        /// <param name="value2">Value being searched for.</param>
        /// <returns>Boolean value indicating if the first string contains the second string.</returns>
        public static bool CaseInsensitiveContains(this string value1, string value2)
        {
            if (value1 == null)
                return false;

            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(value1, value2, CompareOptions.IgnoreCase) > -1;
        }

        /// <summary>
        /// Determines if the first value starts with the second value and ignores case.
        /// </summary>
        /// <param name="value1">Value to be searched.</param>
        /// <param name="value2">Value to be searched for.</param>
        /// <returns>Boolean indicating if the first string starts with the second string.</returns>
        public static bool CaseInsensitiveStartsWith(this string value1, string value2)
        {
            if (value1 == null)
                return false;

            return value1.StartsWith(value2, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Extension method that performs a string.Format.
        /// </summary>
        /// <param name="format">Value containing the format of the output string.</param>
        /// <param name="args">Values to be inserted into format string.</param>
        /// <returns>Formatted string containing provided values.</returns>
        public static string FormatString(this string format, params object[] args)
        {
            if (format == null)
                return null;

            return string.Format(format, args);
        }

        /// <summary>
        /// Changes the casing of the specified string to be in camel case format (camelCaseFormat).
        /// </summary>
        /// <param name="value">Value to be formatted as camel case.</param>
        /// <returns>String containing camel case representation of provided string.</returns>
        public static string ToCamelCase(this string value)
        {
            // Return null if the value is null.
            if (value == null)
                return null;

            // Return a lowercased single letter for values with only one character.
            if (value.Length == 1)
                return value.ToLower();

            // We can't capitalize the first letter of words if they are all lowercase.
            // The simplest way is just to lowercase the first letter of the incoming value.
            // Spaces aren't allowed anyways (for what we're using this for).
            return string.Concat(value.Substring(0, 1).ToLower(), value.Substring(1));
        }

        /// <summary>
        /// Changes the casing of the specified string to be in proper case format (ProperCaseFormat).
        /// </summary>
        /// <param name="value">Value to be formatted as proper case.</param>
        /// <returns>String containing proper case representation of provided string.</returns>
        public static string ToProperCase(this string value)
        {
            if (value == null)
                return null;

            if (value.Length > 1)
                return char.ToUpper(value[0]) + value.Substring(1);

            return value.ToUpper();
        }

        /// <summary>
        /// Encodes the provided string as base 64.
        /// </summary>
        /// <param name="value">String to be encoded as base 64.</param>
        /// <returns>Base 64 encoded version of provided value.</returns>
        public static string ToBase64(this string value)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decodes the provided string from base 64.
        /// </summary>
        /// <param name="value">Base 64 encoded string to be decoded.</param>
        /// <returns>Base 64 decoded version of provided value.</returns>
        public static string FromBase64(this string value)
        {
            var base64EncodedBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}