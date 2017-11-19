//using NuGet.Protocol.Core.v2;
using NuGet.Common;
using System.Diagnostics;
using System;
using System.Threading.Tasks;

namespace Subroute.Core.Nuget
{
    public class TraceLogger : ILogger
    {
        public void Log(LogLevel level, string data)
        {
            throw new NotImplementedException();
        }

        public void Log(ILogMessage message)
        {
            throw new NotImplementedException();
        }

        public Task LogAsync(LogLevel level, string data)
        {
            throw new NotImplementedException();
        }

        public Task LogAsync(ILogMessage message)
        {
            throw new NotImplementedException();
        }

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
