using Subroute.Core.Models.Compiler;

namespace Subroute.Core.Compiler
{
    public class Source
    {
        public Source()
        {
            
        }

        public Source(string code)
        {
            Code = code;
        }

        public Source(string code, Dependency[] dependencies)
            : this(code)
        {
            Dependencies = dependencies;
        }

        public Dependency[] Dependencies { get; set; }
        public string Code { get; set; }
    }
}