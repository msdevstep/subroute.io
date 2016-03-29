using System;
using System.Diagnostics;
using System.Globalization;

namespace Subroute.Core.Tracing
{
    public class TimerTracer : TracerBase
    {
        private readonly Stopwatch _timer = new Stopwatch();
        private readonly DateTimeOffset _start;

        public TimerTracer()
        {
            _start = DateTimeOffset.Now;
            _timer.Start();
        }

        public TimeSpan Elapsed => _timer.Elapsed;

        public TimeSpan Complete()
        {
            _timer.Stop();
            return _timer.Elapsed;
        }

        public override DateTimeOffset GetStartDate()
        {
            return _start;
        }

        public override string GetOutput()
        {
            var milli = Convert.ToInt32(_timer.Elapsed.TotalMilliseconds);
            return milli.ToString(CultureInfo.InvariantCulture);
        }

        public override void Dispose()
        {
            Complete();
        }
    }
}