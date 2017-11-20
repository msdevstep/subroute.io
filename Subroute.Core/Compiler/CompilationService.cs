using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Spatial;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;
using Subroute.Core.Exceptions;
using Subroute.Core.Models.Compiler;
using Subroute.Core.Extensions;
using Microsoft.CodeAnalysis.Completion;
using System.Collections.Concurrent;
using System.Composition.Hosting;
using System.Reflection;
using Microsoft.CodeAnalysis.Host.Mef;
using Subroute.Core.Nuget;

namespace Subroute.Core.Compiler
{
    public interface ICompilationService
    {
        CompilationResult Compile(Source source);
        Task<CompletionResult[]> GetCompletionsAsync(CompletionRequest request, Source source);
    }

    public class CompilationService : ICompilationService
    {
        static IDictionary<string, string> DocumentationFileCache = new ConcurrentDictionary<string, string>();

        private readonly IMetadataProvider _metadataProvider = null;

        public CompilationService(IMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider;
        }

        public CompilationResult Compile(Source source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrWhiteSpace(source.Code))
                throw new CompilationException("No source code was provided.");

            // Build a syntax tree of provided source code, this will allow CodeAnalysis to properly understand the code.
            var tree = CSharpSyntaxTree.ParseText(source.Code);

            // We need to ensure all dependencies have been downloaded and located. ResolveReferences() gets a
            // firm reference for each reference and any additional dependencies.
            var references = _metadataProvider.ResolveReferences(source.Dependencies);
            
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
                        var lineSpan = d.Location.GetLineSpan();
                        var startPostion = lineSpan.StartLinePosition;
                        var endPosition = lineSpan.EndLinePosition;

                        return new Models.Compiler.Diagnostic
                        {
                            Severity = d.Severity,
                            Code = d.Id,
                            Description = d.GetMessage(),
                            Start = new CursorPosition
                            {
                                Line = startPostion.Line,
                                Character = startPostion.Character
                            },
                            End = new CursorPosition
                            {
                                Line = endPosition.Line,
                                Character = endPosition.Character
                            }
                        };
                    }).ToArray()
                };
            }
        }

        public async Task<CompletionResult[]> GetCompletionsAsync(CompletionRequest request, Source source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // We need to ensure all dependencies have been downloaded and located. ResolveReferences() gets a
            // firm reference for each reference and any additional dependencies.
            var references = _metadataProvider.ResolveReferences(source.Dependencies);
            var referenceAssemblies = references.Select(r => Assembly.LoadFrom(r.Display)).ToArray();

            // Setup a workspace for this code file, since we aren't actively managing a project or solutions.
            var workspace = new AdhocWorkspace();
            var projectName = RandomString(6, "Project");
            var assemblyName = RandomString(6, "Assembly");
            var documentName = RandomString(6, "Document");

            var document = workspace.CurrentSolution
                .AddProject(projectName, assemblyName, LanguageNames.CSharp)
                .WithMetadataReferences(references)
                .AddDocument(documentName, SourceText.From(source.Code));
        
            // Determine position of cursor so we know which symbols to recommend.
            var text = await document.GetTextAsync();
            var position = text.Lines.GetPosition(new LinePosition(request.Line, request.Character));
            
            // The code analysis libraries have rolled in all the intellisense code
            // so all we need to do is get a reference to the CompletionService and
            // request completions for the given character position.
            var service = CompletionService.GetService(document);
            var completions = await service.GetCompletionsAsync(document, position);
            var completionResults = new List<CompletionResult>();

            // We'll handle special cases for keywords, and determine whether the
            // completions we received are valid for the given text position.
            if (completions != null)
            {
                foreach (var item in completions.Items)
                {
                    if (item.Tags.Contains(CompletionTags.Keyword))
                    {
                        // For keywords we'll assume the completion text is the same as the display text.
                        var keyword = item.DisplayText;

                        if (keyword.IsValidCompletionFor(request.WordToComplete))
                        {
                            var response = new CompletionResult()
                            {
                                CompletionText = item.DisplayText,
                                DisplayText = item.DisplayText,
                                Snippet = item.DisplayText,
                                Kind = request.WantKind ? "Keyword" : null
                            };

                            completionResults.Add(response);
                        }
                    }
                }
            }
            
            // Now we'll add completions based on the semantics model and referenced assemblies.
            var model = await document.GetSemanticModelAsync();
            var symbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(model, position, workspace);

            foreach (var symbol in symbols.Where(s => s.Name.IsValidCompletionFor(request.WordToComplete)))
            {
                if (request.WantSnippet)
                {
                    foreach (var completion in MakeSnippetedResponses(request, symbol))
                    {
                        completionResults.Add(completion);
                    }
                }
                else
                {
                    completionResults.Add(MakeAutoCompleteResponse(request, symbol));
                }
            }
            
            // Order the list to the most appropriate completions first.
            return completionResults
                .OrderByDescending(c => c.CompletionText.IsValidCompletionStartsWithExactCase(request.WordToComplete))
                .ThenByDescending(c => c.CompletionText.IsValidCompletionStartsWithIgnoreCase(request.WordToComplete))
                .ThenByDescending(c => c.CompletionText.IsCamelCaseMatch(request.WordToComplete))
                .ThenByDescending(c => c.CompletionText.IsSubsequenceMatch(request.WordToComplete))
                .ThenBy(c => c.CompletionText)
                .ToArray();
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

    public class MetadataReferenceEqualityComparer : IEqualityComparer<MetadataReference>
    {
        public bool Equals(MetadataReference x, MetadataReference y)
        {
            return Path.GetFileName(x.Display) == Path.GetFileName(y.Display);
        }

        public int GetHashCode(MetadataReference obj)
        {
            return 0;
        }
    }
}
