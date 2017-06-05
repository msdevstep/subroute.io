using System.Xml.Linq;
using NuGet.Packaging;
using NuGet.ProjectManagement;

namespace Subroute.Core.Nuget
{
    internal class NuGetProjectContext : INuGetProjectContext
    {
        public void Log(MessageLevel level, string message, params object[] args)
        {
            
        }

        public FileConflictAction ResolveFileConflict(string message) => FileConflictAction.Ignore;

        public PackageExtractionContext PackageExtractionContext { get; set; }

        public XDocument OriginalPackagesConfig { get; set; }

        public ISourceControlManagerProvider SourceControlManagerProvider => null;

        ExecutionContext INuGetProjectContext.ExecutionContext => null;

        public void ReportError(string message)
        {
        }

        public NuGetActionType ActionType { get; set; }

        public TelemetryServiceHelper TelemetryService { get; set; }
    }
}