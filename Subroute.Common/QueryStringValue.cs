using System.Linq;

namespace Subroute.Common
{
    public class QueryStringValue
    {
        public QueryStringValue(string[] values)
        {
            Values = values ?? new string[0];
            FirstValue = Values.FirstOrDefault();
        }

        public string FirstValue { get; }
        public string[] Values { get; }

        public static implicit operator string(QueryStringValue value)
        {
            return value.FirstValue;
        }
    }
}