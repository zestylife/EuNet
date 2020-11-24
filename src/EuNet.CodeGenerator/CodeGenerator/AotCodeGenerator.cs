using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeWriter;
using EuNet.Core;

namespace CodeGenerator
{
    public class AotCodeGenerator
    {
        public Options Options { get; set; }

        public void GenerateCode(Type[] types, CodeWriter.CodeWriter w)
        {
            HashSet<string> hashSet = new HashSet<string>();

            foreach (var type in types)
            {
                var baseTypes = type.GetInterfaces().Where(t => t.FullName != "EuNet.Rpc.IRpc").ToArray();
                foreach (var t in new[] { type }.Concat(baseTypes))
                {
                    foreach(var m in t.GetMethods())
                    {
                        if (m.ReturnType.Name.StartsWith("Task") &&
                            m.ReturnType.IsGenericType)
                        {
                            Add(m.ReturnType, hashSet);
                        }

                        foreach(var p in m.GetParameters())
                        {
                            Add(p.ParameterType, hashSet);
                        }
                    }
                }
            }

            var resolverNamespace = $"{Options.Namespace}.AOT";

            w._($"#region {resolverNamespace}");
            w._();

            using (w.B($"namespace {resolverNamespace}"))
            {
                using (w.B($"public sealed class AotCode"))
                {
                    using (w.B("private void UsedOnlyForAOTCodeGeneration()"))
                    {
                        foreach(var code in hashSet)
                        {
                            w._(code);
                        }

                        w._("");
                        w._("throw new InvalidOperationException(\"This method is used for AOT code generation only.Do not call it at runtime.\");");
                    }
                }
            }

            w._();
            w._($"#endregion");
        }

        private void Add(Type type, HashSet<string> hashSet)
        {
            if (type.IsEnum)
            {
                hashSet.Add($"new GenericEnumFormatter<{type.GetPureName()}>();");
            }

            if (type.IsGenericType)
            {
                AddGeneric(type, "System", "Array", hashSet);
                AddGeneric(type, "System", "ArraySegment", hashSet);
                AddGeneric(type, "System", "Tuple", hashSet);
                AddGeneric(type, "System", "ValueTuple", hashSet);

                AddGeneric(type, "System.Collections.Generic", "Dictionary", hashSet);
                AddGeneric(type, "System.Collections.Generic", "List", hashSet);
                AddGeneric(type, "System.Collections.Generic", "LinkedList", hashSet);
                AddGeneric(type, "System.Collections.Generic", "Queue", hashSet);
                AddGeneric(type, "System.Collections.Generic", "Stack", hashSet);
                AddGeneric(type, "System.Collections.Generic", "HashSet", hashSet);

                AddGeneric(type, "System.Collections.Concurrent", "ConcurrentBag", hashSet);
                AddGeneric(type, "System.Collections.Concurrent", "ConcurrentQueue", hashSet);
                AddGeneric(type, "System.Collections.Concurrent", "ConcurrentStack", hashSet);

                foreach (var t in type.GenericTypeArguments)
                {
                    if(t.IsEnum)
                        hashSet.Add($"new GenericEnumFormatter<{t.GetPureName()}>();");

                    if (t.IsGenericType)
                        Add(t, hashSet);
                }
            }
        }

        private void AddGeneric(Type type, string namespaceName, string typeName, HashSet<string> hashSet)
        {
            if (type.FullName.StartsWith($"{namespaceName}.{typeName}"))
            {
                hashSet.Add($"new {typeName}Formatter{type.GetGenericParameters()}();");
            }
        }
    }
}
