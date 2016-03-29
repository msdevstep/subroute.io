using System;
using System.Collections.Generic;
using System.Composition;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.ServiceModel;
using System.Spatial;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;
using Subroute.Core.Compiler.Intellisense;
using Subroute.Core.Exceptions;
using Subroute.Core.Extensions;

namespace Subroute.Core.Compiler
{
    public interface ICompilationService
    {
        CompilationResult Compile(string code);
        Task<CompletionResult[]> GetCompletionsAsync(CompletionRequest request, string code);
    }

    public class CompilationService : ICompilationService
    {
        public CompilationResult Compile(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new CompilationException("No source code was provided.");

            // Build a syntax tree of provided source code, this will allow CodeAnalysis to properly under stand the code.
            var tree = CSharpSyntaxTree.ParseText(code);

            // We need to reference assemblies that the code relies on.
            var references = GetMetadataReferences();

            // Compile syntax tree into a new compilation of a DLL, since we have no need for an entry point.
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var compilationName = $"Assembly_{Guid.NewGuid()}";
            var compilation = CSharpCompilation.Create(compilationName, new[] { tree }, references, options);

            // Emit the assembly into a memory stream and convert the memory stream into a base64 encoded string and return.
            using (var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);

                return new CompilationResult
                {
                    Success = emitResult.Success,
                    Assembly = ms.ToArray(),
                    Diagnostics = emitResult.Diagnostics.Select(d =>
                    {
                        var linePos = d.Location.GetLineSpan().StartLinePosition;

                        return new Diagnostic
                        {
                            Severity = d.Severity,
                            Code = d.Id,
                            Description = d.GetMessage(),
                            Line = linePos.Line + 1,
                            Character = linePos.Character
                        };
                    }).ToArray()
                };
            }
        }

        public async Task<CompletionResult[]> GetCompletionsAsync(CompletionRequest request, string code)
        {
            var workspace = new AdhocWorkspace();
            var projectName = RandomString(6, "Project");
            var assemblyName = RandomString(6, "Assembly");
            var documentName = RandomString(6, "Document");
            var project = workspace.CurrentSolution.AddProject(projectName, assemblyName, LanguageNames.CSharp);

            // We need to reference assemblies that the code relies on.
            var references = GetMetadataReferences();

            project = project.WithMetadataReferences(references);

            var document = project.AddDocument(documentName, SourceText.From(code));
            
            var text = await document.GetTextAsync();
            var position = text.Lines.GetPosition(new LinePosition(request.Line, request.Character));
            var model = await document.GetSemanticModelAsync();
            var completions = new List<CompletionResult>();

            AddKeywords(workspace, completions, model, position, request.WantKind, request.WordToComplete);

            var symbols = Recommender.GetRecommendedSymbolsAtPosition(model, position, workspace);

            foreach (var symbol in symbols.Where(s => s.Name.IsValidCompletionFor(request.WordToComplete)))
            {
                if (request.WantSnippet)
                {
                    foreach (var completion in MakeSnippetedResponses(request, symbol))
                        completions.Add(completion);
                }
                else
                {
                    completions.Add(MakeAutoCompleteResponse(request, symbol));
                }
            }

            return completions
                .OrderByDescending(c => c.CompletionText.IsValidCompletionStartsWithExactCase(request.WordToComplete))
                .ThenByDescending(c => c.CompletionText.IsValidCompletionStartsWithIgnoreCase(request.WordToComplete))
                .ThenByDescending(c => c.CompletionText.IsCamelCaseMatch(request.WordToComplete))
                .ThenByDescending(c => c.CompletionText.IsSubsequenceMatch(request.WordToComplete))
                .ThenBy(c => c.CompletionText)
                .ToArray();
        }

        private static PortableExecutableReference[] GetMetadataReferences()
        {
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof (Uri).Assembly.Location),                           // System
                MetadataReference.CreateFromFile(typeof (object).Assembly.Location),                        // mscorlib
                MetadataReference.CreateFromFile(typeof (Enumerable).Assembly.Location),                    // System.Core
                MetadataReference.CreateFromFile(typeof (ConfigurationManager).Assembly.Location),          // System.Configuration
                MetadataReference.CreateFromFile(typeof (JObject).Assembly.Location),                       // Newtonsoft.Json
                MetadataReference.CreateFromFile(typeof (HttpClient).Assembly.Location),                    // System.Net.Http
                MetadataReference.CreateFromFile(typeof (HttpContentExtensions).Assembly.Location),         // System.Net.Http.Formatting
                MetadataReference.CreateFromFile(typeof (Common.RouteRequest).Assembly.Location),       // Subroute.Common
                MetadataReference.CreateFromFile(typeof (XmlElement).Assembly.Location),                    // System.Xml
                MetadataReference.CreateFromFile(typeof (XObject).Assembly.Location),                       // System.Xml.Linq
                MetadataReference.CreateFromFile(typeof (DataContractAttribute).Assembly.Location),         // System.Runtime.Serialization
                MetadataReference.CreateFromFile(typeof (BasicHttpBinding).Assembly.Location),              // System.ServiceModel
                MetadataReference.CreateFromFile(typeof (DataSet).Assembly.Location),                       // System.Data
                MetadataReference.CreateFromFile(typeof (DataRowExtensions).Assembly.Location),             // System.Data.DataSetExtensions
                MetadataReference.CreateFromFile(typeof (Geography).Assembly.Location),                     // System.Spatial
            };
            return references;
        }

        private void AddKeywords(Workspace workspace, List<CompletionResult> completions, SemanticModel model, int position, bool wantKind, string wordToComplete)
        {
            var context = CSharpSyntaxContext.CreateContext(workspace, model, position, CancellationToken.None);
            var keywordHandler = new KeywordContextHandler();
            var keywords = keywordHandler.Get(context, model, position);

            foreach (var keyword in keywords.Where(k => k.IsValidCompletionFor(wordToComplete)))
            {
                completions.Add(new CompletionResult
                {
                    CompletionText = keyword,
                    DisplayText = keyword,
                    Snippet = keyword,
                    Kind = wantKind ? "Keyword" : null
                });
            }
        }

        private IEnumerable<CompletionResult> MakeSnippetedResponses(CompletionRequest request, ISymbol symbol)
        {
            var completions = new List<CompletionResult>();
            var methodSymbol = symbol as IMethodSymbol;
            if (methodSymbol != null)
            {
                if (methodSymbol.Parameters.Any(p => p.IsOptional))
                {
                    completions.Add(MakeAutoCompleteResponse(request, symbol, false));
                }
                completions.Add(MakeAutoCompleteResponse(request, symbol));
                return completions;
            }
            var typeSymbol = symbol as INamedTypeSymbol;
            if (typeSymbol != null)
            {
                completions.Add(MakeAutoCompleteResponse(request, symbol));

                if (typeSymbol.TypeKind != TypeKind.Enum)
                {
                    foreach (var ctor in typeSymbol.InstanceConstructors)
                    {
                        completions.Add(MakeAutoCompleteResponse(request, ctor));
                    }
                }
                return completions;
            }
            return new[] { MakeAutoCompleteResponse(request, symbol) };
        }

        private CompletionResult MakeAutoCompleteResponse(CompletionRequest request, ISymbol symbol, bool includeOptionalParams = true)
        {
            var displayNameGenerator = new SnippetGenerator();
            displayNameGenerator.IncludeMarkers = false;
            displayNameGenerator.IncludeOptionalParameters = includeOptionalParams;

            var response = new CompletionResult();
            response.CompletionText = symbol.Name;

            if (request.WantKind)
            {
                response.Kind = symbol.GetKind();
            }

            // Handle special case for constructors.
            var methodSymbol = symbol as IMethodSymbol;
            if (methodSymbol?.MethodKind == MethodKind.Constructor)
            {
                response.CompletionText = methodSymbol.ContainingType.Name;

                if (request.WantKind)
                    response.Kind = "Class";
            }

            // TODO: Do something more intelligent here
            response.DisplayText = displayNameGenerator.Generate(symbol);

            if (request.WantDocumentationForEveryCompletionResult)
            {
                response.Description = DocumentationConverter.ConvertDocumentation(symbol.GetDocumentationCommentXml(), "\n");
            }

            if (request.WantReturnType)
            {
                response.ReturnType = ReturnTypeFormatter.GetReturnType(symbol);
            }

            if (request.WantSnippet)
            {
                var snippetGenerator = new SnippetGenerator();
                snippetGenerator.IncludeMarkers = true;
                snippetGenerator.IncludeOptionalParameters = includeOptionalParams;
                response.Snippet = snippetGenerator.Generate(symbol);
            }

            if (request.WantMethodHeader)
            {
                response.MethodHeader = displayNameGenerator.Generate(symbol);
            }

            return response;
        }

        private string RandomString(int length, string prefix = null)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxy";
            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{prefix}_{randomString}";
        }
    }
}
