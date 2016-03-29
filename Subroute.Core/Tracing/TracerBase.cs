using System;

namespace Subroute.Core.Tracing
{
    public abstract class TracerBase : IDisposable
    {
        public abstract DateTimeOffset GetStartDate();
        public abstract string GetOutput();
        public abstract void Dispose();
    }
}