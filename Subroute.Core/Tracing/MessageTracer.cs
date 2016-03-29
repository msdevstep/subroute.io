using System;

namespace Subroute.Core.Tracing
{
    public class MessageTracer : TracerBase
    {
        private readonly string _message;
        private readonly DateTimeOffset _start;

        public MessageTracer(string message)
        {
            _start = DateTimeOffset.Now;
            _message = message;
        }

        public override DateTimeOffset GetStartDate()
        {
            return _start;
        }

        public override string GetOutput()
        {
            return _message;
        }

        public override void Dispose()
        {
            
        }
    }
}