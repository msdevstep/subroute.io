using System;
using System.Diagnostics;

namespace Subroute.Core.Utilities
{
    public static class TraceUtility
    {
        public static TResult TraceTime<TResult>(string name, Func<TResult> action)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = action();
            stopwatch.Stop();

            Trace.TraceInformation($"Trace '{name}' - Elapsed {stopwatch.ElapsedMilliseconds}");

            return result;
        }

        public static void TraceTime(string name, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();

            Trace.TraceInformation($"Trace '{name}' - Elapsed {stopwatch.ElapsedMilliseconds}");
        }
    }
}