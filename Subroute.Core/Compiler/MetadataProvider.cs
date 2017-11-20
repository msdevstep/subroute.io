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
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System.Web.Hosting;
using Subroute.Core.Nuget;
using Subroute.Core.Models.Compiler;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Drawing;

namespace Subroute.Core.Compiler
{
    public interface IMetadataProvider
    {
        MetadataReference[] ResolveReferences(Dependency[] dependencies);
        MetadataReference[] ResolvePackageReferences(Dependency dependency);
    }

    /// <summary>
    /// Resolves framework and package dependencies to actual file locations for the compile engine.
    /// </summary>
    public class MetadataProvider : IMetadataProvider
    {
        private readonly INugetService _NugetService;
        private IDictionary<string, string> _DocumentationLookup = null;
        private readonly NugetPackageComparer _NugetPackageComparer = new NugetPackageComparer();
    
        public MetadataProvider(INugetService nugetService)
        {
            _NugetService = nugetService;
        }

        private IDictionary<string, string> GetDocumentationLookup()
        {
            // Return lookup cache if one exists.
            if (_DocumentationLookup != null)
                return _DocumentationLookup;

            // Locate all xml files in app path and cache them in a concurrent dictionary.
            // These file will be the assembly documentation files we need to provide xml
            // documentation to the intellisense front-end.
            var appPath = HostingEnvironment.ApplicationPhysicalPath;
            var searchDirectories = new List<string>(Directory.EnumerateDirectories(appPath, "*", SearchOption.AllDirectories));

            // Add additional search locations for the framework assemblies.
            var programFiles = Environment.Is64BitOperatingSystem ? Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles;
            var frameworkPath = Path.Combine(Environment.GetFolderPath(programFiles), Settings.DocumentationPath);

            searchDirectories.Add(frameworkPath);

            // Create new concurrent dictionary to act as our doc lookup cache.
            _DocumentationLookup = new ConcurrentDictionary<string, string>();

            foreach (var path in searchDirectories)
            {
                foreach (var file in Directory.GetFiles(path, "*.xml"))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var filename = Path.GetFileName(file);

                    if (!_DocumentationLookup.ContainsKey(name))
                        _DocumentationLookup.Add(name, Path.Combine(path, filename));
                }
            }

            // Return the new concurrent dictionary to use for lookup.
            return _DocumentationLookup;
        }

        /// <summary>
        /// Gets standard framework dependencies and resolves any package dependencies passed in as
        /// MetadataReferences to provide the actual assembly file and documentation file together.
        /// </summary>
        /// <param name="dependencies">Package dependencies to be located on the file system.</param>
        /// <returns>An array of <see cref="MetadataReference"/> containing all resolved assemblies and documentation.</returns>
        public MetadataReference[] ResolveReferences(Dependency[] dependencies)
        {
            // Ensure we never have a null reference for the dependency list.
            if (dependencies == null)
                throw new ArgumentNullException(nameof(dependencies));
            
            var assemblies = new[]
            {
                typeof (Uri).Assembly,
                typeof (object).Assembly,
                typeof (Enumerable).Assembly,
                typeof (ConfigurationManager).Assembly,
                typeof (JObject).Assembly,
                typeof (HttpClient).Assembly,
                typeof (HttpContentExtensions).Assembly,
                typeof (Common.RouteRequest).Assembly,
                typeof (XmlElement).Assembly,
                typeof (XObject).Assembly,
                typeof (DataContractAttribute).Assembly,
                typeof (BasicHttpBinding).Assembly,
                typeof (DataSet).Assembly,
                typeof (DataRowExtensions).Assembly,
                typeof (Geography).Assembly,
                typeof (Size).Assembly
            };

            // Pull in cache containing a list of documentation files to make finding an assemblies associated
            // XML doc file to be added to the MetadataReference. This directory search is only done once on
            // first request.
            var fileLookup = GetDocumentationLookup();

            return assemblies
                .Select(a =>
                {
                    // Locate the name of the assembly to determine what the documentation file name is.
                    var assemblyName = a.GetName().Name;
                    
                    // Attempt to locate an xml documentation file in the cache.
                    if (fileLookup.TryGetValue(assemblyName, out string docPath))
                        return MetadataReference.CreateFromFile(a.Location, documentation: XmlDocumentationProvider.CreateFromFile(docPath));

                    // Otherwise we have no documentation file and we'll return it without a doc provider.
                    return MetadataReference.CreateFromFile(a.Location);
                })
                // Iterate through each dependency to get actual MetadataReferences containing the assembly paths
                // and documentation paths for the assemblies contained with the package folders.
                // There can be multiple assemblies required for each NuGet package, so the ResolvePackageReferencesAsync()
                // method returns an array of MetadataReferences for each of the assemblies and matching documentation files.
                // Add the references to the final output to be included in the compilation.
                .Concat(dependencies.SelectMany(d => ResolvePackageReferences(d)))
                .ToArray();
        }

        /// <summary>
        /// Locate the assembly and documentation files for a given dependency and return an array of the
        /// finalized MetadataReferences for each assembly.
        /// </summary>
        /// <param name="dependency">Dependency package containing the files to be referenced.</param>
        /// <returns>An array of <see cref="MetadataReference"/> containing the assembly and documentation paths.</returns>
        public MetadataReference[] ResolvePackageReferences(Dependency dependency)
        {
            // Find the highest supported assembly folder in the package and get the fully-qualified assembly
            // and documentation path for each located assembly. Convert the PackageReference to a 
            // MetadataReference ready to be consumed by Roslyn.
            return _NugetService
                .GetPackageReferences(dependency)
                .Select(r => r.ToMetadataReference())
                .ToArray();
        }
    }
}
