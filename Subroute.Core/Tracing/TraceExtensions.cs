using System.Net.Http;

namespace Subroute.Core.Tracing
{
    public static class TraceExtensions
    {
        /// <summary>
        /// Creates and starts a timer tracer attached to current request.
        /// </summary>
        /// <param name="context">Current trace context for lifetime scope (request).</param>
        /// <param name="name">Unique name of the timer. Usually related to the current operation being performed. Must contain alphanumeric and hyphen characters only.</param>
        /// <returns><see cref="TracerBase"/> that represents a <see cref="TimerTracer"/>.</returns>
        public static TracerBase StartTimerTracer(this ITraceContext context, string name)
        {
            TraceContext.EnsureValidTracerName(name);

            if (context.Tracers.ContainsKey(name))
            {
                var existingTracer = context.Tracers[name] as TimerTracer;
                existingTracer?.Complete();

                var replacementTracer = new TimerTracer();
                context.Tracers[name] = replacementTracer;
                return replacementTracer;
            }

            var tracer = new TimerTracer();
            context.Tracers.Add(name, tracer);
            return tracer;
        }

        /// <summary>
        /// Finds an existing <see cref="TimerTracer"/> and marks it as complete.
        /// </summary>
        /// <param name="context">Current trace context for lifetime scope (request).</param>
        /// <param name="name">Unique name of the timer. Usually related to the current operation being performed. Must contain alphanumeric and hyphen characters only.</param>
        public static void CompleteTimerTracer(this ITraceContext context, string name)
        {
            TraceContext.EnsureValidTracerName(name);
            TracerBase tracer;

            if (!context.Tracers.TryGetValue(name, out tracer))
                return;

            var typedTracer = tracer as TimerTracer;

            typedTracer?.Complete();
        }

        /// <summary>
        /// Adds the specified message to the trace context and will output the message on the response.
        /// </summary>
        /// <param name="context">Current trace context for lifetime scope (request).</param>
        /// <param name="name">Unique name of the tracer. Usually related to the current operation being performed. Must contain alphanumeric and hyphen characters only.</param>
        /// <param name="message"></param>
        public static void WriteMessageTracer(this ITraceContext context, string name, string message)
        {
            TraceContext.EnsureValidTracerName(name);
            
            var traceContext = (TraceContext)context;
            var tracer = new MessageTracer(message);
            traceContext.Tracers.Add(name, tracer);
        }
        
        internal static void OuputTracerResults(this ITraceContext context, HttpResponseMessage response)
        {
            context.OuputTracerResults(response);
        }
    }
}