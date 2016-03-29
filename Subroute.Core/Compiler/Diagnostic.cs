using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Subroute.Core.Compiler
{
    public class Diagnostic
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public DiagnosticSeverity Severity { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int Line { get; set; }
        public int Character { get; set; }
    }
}