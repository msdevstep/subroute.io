using System.Collections.Generic;

namespace Subroute.Core.Nuget
{
    public class NugetPackageComparer : IEqualityComparer<NugetPackage>
    {
        public bool Equals(NugetPackage x, NugetPackage y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(NugetPackage obj)
        {
            return string.Concat(obj?.Id, obj?.Version).GetHashCode();
        }
    }
}
