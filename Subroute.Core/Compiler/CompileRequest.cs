namespace Subroute.Core.Compiler
{
    public class CompileRequest
    {
        public CompileRequest(string code)
        {
            Code = code;
        }

        public CompileRequest(string code, Dependency[] dependencies)
            : this(code)
        {
            Dependencies = dependencies;
        }

        public Dependency[] Dependencies { get; set; }
        public string Code { get; set; }
    }
}