using System;

namespace Subroute.Core.Tracing
{
    public class IdleTracer : TracerBase
    {
        private readonly DateTimeOffset _start;

        public IdleTracer()
        {
            _start = DateTimeOffset.Now;
        }

        public override DateTimeOffset GetStartDate()
        {
            return _start;
        }

        public override string GetOutput()
        {
            return null;
        }

        public override void Dispose()
        {

        }
    }
}