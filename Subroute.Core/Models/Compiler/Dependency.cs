namespace Subroute.Core.Models.Compiler
{
    public class Dependency
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public DependencyType Type { get; set; }
    }
}