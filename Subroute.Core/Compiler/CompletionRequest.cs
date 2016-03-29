namespace Subroute.Core.Compiler
{
    public class CompletionRequest
    {
        private string _wordToComplete;

        public string WordToComplete
        {
            get { return _wordToComplete ?? string.Empty; }
            set { _wordToComplete = value; }
        }

        public int Line { get; set; }
        public int Character { get; set; }
        public bool WantSnippet { get; set; }
        public bool WantDocumentationForEveryCompletionResult { get; set; }
        public bool WantReturnType { get; set; }
        public bool WantKind { get; set; }
        public bool WantMethodHeader { get; set; }
    }
}