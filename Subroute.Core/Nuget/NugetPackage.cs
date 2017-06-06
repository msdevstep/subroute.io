using System;
using System.Collections.Generic;
using NuGet;
using System.Linq;
using Subroute.Core.Extensions;
using NuGet.Protocol.Core.Types;
using System.Diagnostics;

namespace Subroute.Core.Nuget
{
    [DebuggerDisplay("{Id,nq} {Version,nq}")]
    public class NugetPackage
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public long DownloadCount { get; set; }
        public Uri IconUrl { get; set; }
        public Uri ProjectUrl { get; set; }
        public Uri LicenseUrl { get; set; }
        public string Tags { get; set; }
        public string Version { get; set; }
        public string Owners { get; set; }
        public string Authors { get; set; }
        public DateTimeOffset? PublishedOn { get; set; }

        public static Func<IPackageSearchMetadata, NugetPackage> Map => p => new NugetPackage
        {
            Id = p?.Identity?.Id,
            Version = p?.Identity?.Version?.ToFullString(),
            Title = p.Title,
            Summary = p.Summary,
            Description = p.Description,
            DownloadCount = p.DownloadCount.GetValueOrDefault(),
            IconUrl = p.IconUrl,
            ProjectUrl = p.ProjectUrl,
            LicenseUrl = p.LicenseUrl,
            Tags = p.Tags,
            PublishedOn = p.Published,
            Owners = p.Owners,
            Authors = p.Authors
        };
    }
}
