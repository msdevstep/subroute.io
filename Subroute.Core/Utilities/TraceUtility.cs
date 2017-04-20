using System;
using System.Diagnostics;
using System.Threading.Tasks;

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

        public static TResult TraceTime<TResult>(this Task<TResult> task, string name)
        {
            var stopwatch = Stopwatch.StartNew();
            task.Wait();
            stopwatch.Stop();

            Trace.TraceInformation($"Trace '{name}' - Elapsed {stopwatch.ElapsedMilliseconds}");

            return task.Result;
        }

        public static async Task<TResult> TraceTimeAsync<TResult>(this Task<TResult> task, string name)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await task;
            stopwatch.Stop();

            Trace.TraceInformation($"Trace '{name}' - Elapsed {stopwatch.ElapsedMilliseconds}");

            return result;
        }

        public static void TraceTime(this Task task, string name)
        {
            var stopwatch = Stopwatch.StartNew();
            task.Wait();
            stopwatch.Stop();

            Trace.TraceInformation($"Trace '{name}' - Elapsed {stopwatch.ElapsedMilliseconds}");
        }

        public static async Task TraceTimeAsync(this Task task, string name)
        {
            var stopwatch = Stopwatch.StartNew();
            await task;
            stopwatch.Stop();

            Trace.TraceInformation($"Trace '{name}' - Elapsed {stopwatch.ElapsedMilliseconds}");
        }
    }
}