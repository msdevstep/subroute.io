using System;

namespace Subroute.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static int ToEpoch(this DateTime dateTime)
        {
            return Convert.ToInt32((dateTime - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static DateTimeOffset FromEpoch(this int epochTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(epochTime);
        }
    }
}