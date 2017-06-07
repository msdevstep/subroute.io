

using System;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Subroute.Core.Data;

namespace Subroute.Core.Models.Compiler
{
    public class Dependency
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public DependencyType Type { get; set; }

        public PackageIdentity ToPackageIdentity()
        {
            if (Type != DependencyType.NuGet)
                throw new Exception("Dependency is not a NuGet package and cannot by converted to a PackageIdentity.");

            return new PackageIdentity(Id, new NuGetVersion(Version));
        }

        public static Dependency FromRoutePackage(RoutePackage package)
        {
            return new Dependency
            {
                Id = package.Id,
                Version = package.Version,
                Type = package.Type
            };
        }
    }
}