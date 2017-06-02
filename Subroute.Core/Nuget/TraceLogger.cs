//using NuGet.Protocol.Core.v2;
using NuGet.Common;
using System.Diagnostics;

namespace Subroute.Core.Nuget
{
    public class TraceLogger : ILogger
    {
        public void LogDebug(string data)
        {
            Trace.TraceInformation(data);
        }

        public void LogError(string data)
        {
            Trace.TraceError(data);
        }

        public void LogErrorSummary(string data)
        {
            Trace.TraceError(data);
        }

        public void LogInformation(string data)
        {
            Trace.TraceInformation(data);
        }

        public void LogInformationSummary(string data)
        {
            Trace.TraceInformation(data);
        }

        public void LogMinimal(string data)
        {
            Trace.TraceInformation(data);
        }

        public void LogVerbose(string data)
        {
            Trace.TraceInformation(data);
        }

        public void LogWarning(string data)
        {
            Trace.TraceWarning(data);
        }
    }
}
