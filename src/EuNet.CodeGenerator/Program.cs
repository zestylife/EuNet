using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommandLine;

namespace CodeGenerator
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // Parse command line options

            if (args.Length == 1 && args[0].StartsWith("@"))
            {
                var argFile = args[0].Substring(1);
                if (File.Exists(argFile))
                {
                    args = File.ReadAllLines(argFile);
                }
                else
                {
                    Console.WriteLine("File not found: " + argFile);
                    return 1;
                }
            }

            var parser = new Parser(config => config.HelpWriter = Console.Out);
            if (args.Length == 0)
            {
                parser.ParseArguments<Options>(new[] { "--help" });
                return 1;
            }

            Options options = null;
            var result = parser.ParseArguments<Options>(args)
                .WithParsed(r => { options = r; });

            // Run process !

            if (options != null)
                return Process(options);
            else
                return 1;
        }

        private static int Process(Options options)
        {
            try
            {
                Console.WriteLine("- Start Process!");

                // Resolve options

                var basePath = Path.GetFullPath(options.Path ?? ".");
                var sources = options.Sources.Where(p => string.IsNullOrWhiteSpace(p) == false && FilterSource(options, p)).Select(p => MakeFullPath(p, basePath)).ToArray();
                var references = options.References.Where(p => string.IsNullOrWhiteSpace(p) == false).Select(p => MakeFullPath(p, basePath)).ToArray();
                var defines = options.Defines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                var targetDefaultPath = @".\CodeGen\EuNet.Rpc.CodeGen.cs";
                var targetPath = MakeFullPath(options.TargetFile ?? targetDefaultPath, basePath);

                if(options.TargetFile == null)
                {
                    DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(targetDefaultPath));
                    if (di.Exists == false)
                        di.Create();
                }

                // Build source and load assembly
                Console.WriteLine("- Build sources");
                
                var assembly = AssemblyLoader.BuildAndLoad(sources, references, defines);
                if (assembly == null)
                {
                    Console.WriteLine("Fail to AssemblyLoader.BuildAndLoad");
                    return 1;
                }

                // Generate code

                Console.WriteLine("- Generate code");

                var codeGenerator = new EntryCodeGenerator(options);
                var sourceTypes = GetTypesSafely(assembly).OrderBy(t => t.FullName).ToArray();
                codeGenerator.GenerateCode(sourceTypes);

                // Save generated code

                Console.WriteLine("- Save code");

                if (codeGenerator.CodeWriter.WriteAllText(targetPath, true) == false)
                    Console.WriteLine("Nothing changed. Skip writing.");

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in processing:\n" + e);
                return 1;
            }
        }

        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine($"GetTypesSafely({assembly.GetName()}) got ReflectionTypeLoadException");
                return ex.Types.Where(x => x != null);
            }
        }

        private static bool FilterSource(Options options, string path)
        {
            if (path.ToLower().IndexOf("eunet.rpc.codegen") != -1)
                return false;

            if (path.ToLower().IndexOf("globalusings.g.cs") != -1)
                return false;

            foreach (var exclude in options.Excludes)
            {
                if (Regex.IsMatch(path, exclude, RegexOptions.IgnoreCase))
                    return false;
            }

            if (options.Includes.Any())
            {
                foreach (var include in options.Includes)
                {
                    if (Regex.IsMatch(path, include, RegexOptions.IgnoreCase))
                        return true;
                }
                return false;
            }

            return true;
        }

        private static string MakeFullPath(string path, string basePath)
        {
            if (Path.IsPathRooted(path))
                return path;
            else
                return Path.Combine(basePath, path);
        }
    }
}
