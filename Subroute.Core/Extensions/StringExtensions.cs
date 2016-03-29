using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subroute.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool CaseInsensitiveEqual(this string value1, string value2)
        {
            return string.Equals(value1, value2, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool CaseInsensitiveContains(this IEnumerable<string> array, string value)
        {
            if (array == null)
                return false;

            return array.Any(a => a.CaseInsensitiveEqual(value));
        }

        public static bool CaseInsensitiveContains(this string value1, string value2)
        {
            if (value1 == null)
                return false;

            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(value1, value2, CompareOptions.IgnoreCase) > -1;
        }

        public static bool CaseInsensitiveStartsWith(this string value1, string value2)
        {
            if (value1 == null)
                return false;

            return value1.StartsWith(value2, StringComparison.CurrentCultureIgnoreCase);
        }

        public static string FormatString(this string format, params object[] args)
        {
            if (format == null)
                return null;

            return string.Format(format, args);
        }

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

        public static string ToProperCase(this string value)
        {
            if (value == null)
                return null;

            if (value.Length > 1)
                return char.ToUpper(value[0]) + value.Substring(1);

            return value.ToUpper();
        }

        public static string ToBase64(this string value)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string FromBase64(this string value)
        {
            var base64EncodedBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static bool IsValidCompletionFor(this string completion, string partial)
        {
            return completion.IsValidCompletionStartsWithIgnoreCase(partial) || completion.IsSubsequenceMatch(partial);
        }

        public static bool IsValidCompletionStartsWithExactCase(this string completion, string partial)
        {
            return completion.StartsWith(partial);
        }

        public static bool IsValidCompletionStartsWithIgnoreCase(this string completion, string partial)
        {
            return completion.ToLower().StartsWith(partial.ToLower());
        }

        public static bool IsCamelCaseMatch(this string completion, string partial)
        {
            return new string(completion.Where(c => c >= 'A' && c <= 'Z').ToArray()).StartsWith(partial.ToUpper());
        }

        public static bool IsSubsequenceMatch(this string completion, string partial)
        {
            if (partial == string.Empty)
            {
                return true;
            }

            // Limit the number of results returned by making sure
            // at least the first characters match.
            // We can get far too many results back otherwise.
            if (!FirstLetterMatches(partial, completion))
            {
                return false;
            }

            return new string(completion.ToUpper().Intersect(partial.ToUpper()).ToArray()) == partial.ToUpper();
        }

        private static bool FirstLetterMatches(string word, string match)
        {
            if (string.IsNullOrEmpty(match))
            {
                return false;
            }

            return char.ToLowerInvariant(word.First()) == char.ToLowerInvariant(match.First());
        }
    }
}
