using System;
using System.Collections.Generic;
using NuGet;
using System.Linq;

namespace Subroute.Core.Nuget
{
    public class NugetPackage
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public int DownloadCount { get; set; }
        public Uri IconUrl { get; set; }
        public Uri ProjectUrl { get; set; }
        public Uri LicenseUrl { get; set; }
        public string Tags { get; set; }
        public string Language { get; set; }
        public string Version { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public IEnumerable<string> Owners { get; set; }
        public DateTimeOffset? PublishedOn { get; set; }

        public static Func<IPackage, NugetPackage> Map => p => new NugetPackage
        {
            Id = p.Id,
            Title = p.Title,
            Summary = p.Summary,
            Description = p.Description,
            DownloadCount = p.DownloadCount,
            IconUrl = p.IconUrl,
            ProjectUrl = p.ProjectUrl,
            LicenseUrl = p.LicenseUrl,
            Tags = p.Tags,
            Version = p.Version?.ToFullString(),
            PublishedOn = p.Published,
            Owners = p.Owners,
            Language = p.Language,
            Authors = p.Authors
        };
    }
}
