using System;
using System.Collections.Generic;
using System.Reflection;
using CodeWriter;
using EuNet.Core;

namespace CodeGenerator
{
    public class ResolverCodeGenerator
    {
        public Options Options { get; set; }

        public void GenerateCode(Dictionary<string, string> formatterMap, CodeWriter.CodeWriter w)
        {
            w._($"#region {Options.ResolverNamespace}");
            w._();

            using (w.B($"namespace {Options.ResolverNamespace}"))
            {
                GenerateResolverCode(formatterMap, w);
            }

            w._();
            w._($"#endregion");
        }

        private void GenerateResolverCode(Dictionary<string, string> formatterMap, CodeWriter.CodeWriter w)
        {
            //var resolverClassName = $"{type.Namespace}Resolver".Replace(".","_");
            //var resolverHelperClassName = $"{type.Namespace}ResolverGetFormatterHelper".Replace(".", "_");
            var resolverClassName = "GeneratedResolver";
            var resolverHelperClassName = $"{resolverClassName}GetFormatterHelper";

            using (w.B($"public sealed class {resolverClassName} : INetDataFormatterResolver"))
            {
                w._($"public static readonly {resolverClassName} Instance = new {resolverClassName}();");
                w._();

                // Constructor
                using (w.B($"private {resolverClassName}()"))
                {
                    
                }

                // Formatter
                using (w.B("public INetDataFormatter<T> GetFormatter<T>()"))
                {
                    w._("return FormatterCache<T>.Formatter;");
                }

                // FormatterCache
                using (w.B("private static class FormatterCache<T>"))
                {
                    w._("public static readonly INetDataFormatter<T> Formatter;");
                    w._();

                    using (w.B("static FormatterCache()"))
                    {
                        w._($"Formatter = (INetDataFormatter<T>){resolverHelperClassName}.GetFormatter(typeof(T));");
                    }
                }
            }

            using (w.B($"internal static class {resolverHelperClassName}"))
            {
                using (w.i("private static readonly Dictionary<Type, object> FormatterMap = new Dictionary<Type, object>() {", "};")) 
                {
                    foreach(var value in formatterMap)
                        w._($"{{ {value.Key} , {value.Value} }},");
                }

                using (w.B("internal static object GetFormatter(Type t)"))
                {
                    w._("TypeInfo ti = t.GetTypeInfo();");

                    using (w.B("if (ti.IsGenericType)"))
                    {
                        w._("Type genericType = ti.GetGenericTypeDefinition();");
                        w._("object formatterType;");

                        using (w.B("if (FormatterMap.TryGetValue(genericType, out formatterType))"))
                        {
                            w._("return Activator.CreateInstance(((Type)formatterType).MakeGenericType(ti.GenericTypeArguments));");
                        }
                    }
                    using (w.B("else"))
                    {
                        w._("object formatter;");
                        using (w.B("if (FormatterMap.TryGetValue(t, out formatter))"))
                        {
                            w._("return formatter;");
                        }
                    }

                    w._("return null;");
                }
            }
        }
    }
}
