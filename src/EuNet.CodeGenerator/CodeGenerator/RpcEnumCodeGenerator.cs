using System.Collections.Generic;
using CodeWriter;

namespace CodeGenerator
{
    public class RpcEnumCodeGenerator
    {
        public Options Options { get; set; }

        public void GenerateCode(Dictionary<int, string> rpcEnumMap, CodeWriter.CodeWriter w)
        {
            var rpcEnumNamespace = $"{Options.Namespace}";

            w._($"#region {rpcEnumNamespace}");
            w._();

            using (w.B($"namespace {rpcEnumNamespace}"))
            {
                using (w.B($"public static class RpcEnumHelper"))
                {
                    using (w.B("public static string GetEnumName(int rpcNameHash)"))
                    {
                        using (w.B($"switch(rpcNameHash)"))
                        {
                            foreach (var kvp in rpcEnumMap)
                            {
                                w._($"case {kvp.Key}: return \"{kvp.Value}\";");
                            }
                        }

                        w._($"return string.Empty;");
                    }
                }
            }

            w._();
            w._($"#endregion");
        }
    }
}
