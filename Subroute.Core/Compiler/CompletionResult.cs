namespace Subroute.Core.Compiler
{
    public class CompletionResult
    {
        public string CompletionText { get; set; }
        public string DisplayText { get; set; }
        public string Description { get; set; }
        public string ReturnType { get; set; }
        public string Snippet { get; set; }
        public string Kind { get; set; }
        public string MethodHeader { get; set; }
    }
}