using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Subroute.Core.Tracing
{
    public interface ITraceContext
    {
        IDictionary<string, TracerBase> Tracers { get; }
        void OuputTracerResults(HttpResponseMessage response);
        TResult TraceTime<TResult>(string name, Func<TResult> action);
        Task TraceTimeAsync(string name, Task task);
        Task<TResult> TraceTimeAsync<TResult>(string name, Task<TResult> task);
    }

    public class TraceContext : ITraceContext
    {
        private readonly IDictionary<string, TracerBase> _tracers = new Dictionary<string, TracerBase>();

        public IDictionary<string, TracerBase> Tracers => _tracers;

        public TResult TraceTime<TResult>(string name, Func<TResult> action)
        {
            using (this.StartTimerTracer(name))
                return action();
        }

        public async Task TraceTimeAsync(string name, Task task)
        {
            using (this.StartTimerTracer(name))
                await task;
        }

        public async Task<TResult> TraceTimeAsync<TResult>(string name, Task<TResult> task)
        {
            using (this.StartTimerTracer(name))
                return await task;
        }

        public void OuputTracerResults(HttpResponseMessage response)
        {
            // Determine padding amount using best match for tracer count.
            var format = "0000";
            if (_tracers.Count < 1000)
                format = "000";
            if (_tracers.Count < 100)
                format = "00";
            if (_tracers.Count < 10)
                format = "0";

            // We need a TelemetryClient to record the various trace types to App Insights.
            var telemetry = new TelemetryClient();
            
            for (var i = 0; i < _tracers.Count; i++)
            {
                var tracerName = _tracers.Keys.ElementAt(i);
                var tracer = _tracers.Values.ElementAt(i);
                var tracerHeaderName = string.Concat("T", i.ToString(format), "-", tracerName);
                var output = tracer.GetOutput();

                // Record timer traces with App Insights.
                var timer = tracer as TimerTracer;
                if (timer != null)
                    telemetry.TrackRequest(tracerName, timer.GetStartDate(), timer.Elapsed, ((int)response.StatusCode).ToString(), response.IsSuccessStatusCode);

                // Record message traces with App Insights.
                var message = tracer as MessageTracer;
                if (message != null)
                    telemetry.TrackTrace(message.GetOutput(), SeverityLevel.Information);

                if (string.IsNullOrWhiteSpace(output))
                    continue;

                EnsureValidTracerName(tracerName);

                response.Headers.Add(tracerHeaderName, output);
            }
        }

        public static void EnsureValidTracerName(string name)
        {
            // Create a regex to ensure the trace header name is only alpha numeric.
            var alphaNumeric = new Regex("^[a-zA-Z0-9-]*$");

            if (!alphaNumeric.IsMatch(name))
                throw new FormatException($"Trace header name {name} is invalid. Only alpha numeric characters are allowed.");
        }
    }
}
