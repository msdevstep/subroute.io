namespace Subroute.Core.Compiler
{
    public class CompilationResult
    {
        public bool Success { get; set; }
        public byte[] Assembly { get; set; }
        public Diagnostic[] Diagnostics { get; set; }
    }
}