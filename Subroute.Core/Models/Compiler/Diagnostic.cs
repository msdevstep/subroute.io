using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Subroute.Core.Models.Compiler
{
    public class Diagnostic
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public DiagnosticSeverity Severity { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public CursorPosition Start { get; set; }
        public CursorPosition End { get; set; }
    }
}