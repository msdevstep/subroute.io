using System;

namespace Subroute.Core.Compiler
{
    public class Dependency
    {
        public Dependency(DependencyType type, string name, Version version)
        {
            Type = type;
            Name = name;
            Version = version;
        }

        public DependencyType Type { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }
    }
}