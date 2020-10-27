using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeGenerator
{
    public static class AssemblyLoader
    {
        private static ResolveEventHandler _lastResolveHandler;

        public static Assembly BuildAndLoad(string[] sourcePaths, string[] referencePaths, string[] defines)
        {
            var assemblyName = Path.GetRandomFileName();
            var parseOption = new CSharpParseOptions(LanguageVersion.CSharp8, DocumentationMode.Parse, SourceCodeKind.Regular, defines);
            var syntaxTrees = sourcePaths.Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file), parseOption, file)).ToArray();
            var references = referencePaths.Select(file => MetadataReference.CreateFromFile(file)).ToArray();
            var referenceMaps = referencePaths.Select(file => { try { return Assembly.LoadFile(file); } catch { } return null; })
                .Where( x => x != null )
                .ToDictionary(a => a.FullName, a => a);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (var diagnostic in failures)
                    {
                        var line = diagnostic.Location.GetLineSpan();
                        Console.Error.WriteLine("{0}({1}): {2} {3}",
                            line.Path,
                            line.StartLinePosition.Line + 1,
                            diagnostic.Id,
                            diagnostic.GetMessage());
                    }
                    return null;
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    // To load referenced assemblies, set customized resolved during using this assembly.

                    var currentDomain = AppDomain.CurrentDomain;
                    var resolveHandler = new ResolveEventHandler((sender, args) =>
                    {
                        Assembly assembly;
                        return referenceMaps.TryGetValue(args.Name, out assembly) ? assembly : null;
                    });

                    if (_lastResolveHandler != null)
                        currentDomain.AssemblyResolve -= _lastResolveHandler;
                    currentDomain.AssemblyResolve += resolveHandler;
                    _lastResolveHandler = resolveHandler;

                    return Assembly.Load(ms.ToArray());
                }
            }
        }
    }
}
