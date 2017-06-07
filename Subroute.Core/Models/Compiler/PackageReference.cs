using System.IO;
using Microsoft.CodeAnalysis;
using Subroute.Core.Extensions;

namespace Subroute.Core.Models.Compiler
{
    public class PackageReference
    {
        public string AssemblyPath { get; set; }
        public string DocumentationPath { get; set; }

        public bool HasDocumentation
        {
            get { return DocumentationPath.HasValue() ? File.Exists(DocumentationPath) : false; }
        }

        public MetadataReference ToMetadataReference()
        {
            var docs = HasDocumentation ? XmlDocumentationProvider.CreateFromFile(DocumentationPath) : null;
            return MetadataReference.CreateFromFile(AssemblyPath, documentation: docs);
        }
    }
}
