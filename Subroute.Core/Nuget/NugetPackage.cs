using System;
using System.Collections.Generic;
using NuGet;

namespace Subroute.Core.Nuget
{
    public class NugetPackage
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Description { get; internal set; }
        public int DownloadCount { get; internal set; }
        public Uri IconUrl { get; internal set; }
        public Uri ProjectUrl { get; internal set; }
        public Uri LicenseUrl { get; internal set; }
        public string Tags { get; internal set; }
        public string Language { get; internal set; }
        public string Version { get; internal set; }
        public IEnumerable<string> Authors { get; internal set; }
        public IEnumerable<string> Owners { get; internal set; }
        public bool IsLatestVersion { get; internal set; }
        public string MinClientVersion { get; internal set; }
        public DateTimeOffset? PublishedOn { get; internal set; }
    }
}
