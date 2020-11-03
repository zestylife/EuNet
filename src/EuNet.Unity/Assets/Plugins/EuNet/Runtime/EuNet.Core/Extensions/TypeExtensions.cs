using System;
using System.Linq;
using System.Reflection;

namespace EuNet.Core
{
    public static class TypeExtensions
    {
        // Example: System.String -> string
        public static string GetSpecialTypeName(this Type type)
        {
            if (type == typeof(void))
                return "void";
            if (type == typeof(sbyte))
                return "sbyte";
            if (type == typeof(short))
                return "short";
            if (type == typeof(int))
                return "int";
            if (type == typeof(long))
                return "long";
            if (type == typeof(byte))
                return "byte";
            if (type == typeof(ushort))
                return "ushort";
            if (type == typeof(uint))
                return "uint";
            if (type == typeof(ulong))
                return "ulong";
            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";
            if (type == typeof(decimal))
                return "decimal";
            if (type == typeof(char))
                return "char";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(string))
                return "string";
            if (type == typeof(object))
                return "object";
            return null;
        }

        // Example: List<T> -> List
        public static string GetPureName(this Type type)
        {
            if (type.IsGenericType)
            {
                var delimiterPos = type.Name.IndexOf('`');
                return type.Name.Substring(0, delimiterPos);
            }
            else
            {
                return GetSpecialTypeName(type) ?? type.Name;
            }
        }

        // Example: List<T> -> List_1
        public static string GetSafeName(this Type type)
        {
            if (type.IsGenericType)
            {
                return type.Name.Replace('`', '_');
            }
            else
            {
                return GetSpecialTypeName(type) ?? type.Name;
            }
        }

        public static int GetTupleSize(Type tuple)
        {
            var genericArguments = tuple.GetGenericArguments();
            if (genericArguments.Length > 7)
            {
                return 7 + GetTupleSize(genericArguments[7]);
            }
            else
            {
                return genericArguments.Length;
            }
        }

        // Example: Dictionary<int, string> -> System.Collections.Generic.Dictionary<System.Int32, System.String>
        public static string GetSymbolDisplay(this Type type, bool isFullName = false, bool typeless = false)
        {
            if (type.IsGenericType)
            {
                var namespacePrefix = type.Namespace + (type.Namespace.Length > 0 ? "." : "");
                return (isFullName ? namespacePrefix : "") + type.GetPureName() + type.GetGenericParameters(typeless);
            }
            else
            {
                return type.GetSpecialTypeName() ?? (isFullName ? (type.FullName ?? type.Name) : type.Name);
            }
        }

        // Output: <T, U>
        public static string GetGenericParameters(this Type type, bool typeless = false)
        {
            if (type.IsGenericType == false)
                return "";
            var genericParams = type.GenericTypeArguments.Any()
                ? string.Join(", ", type.GenericTypeArguments.Select(t => typeless ? "" : t.GetSymbolDisplay(true)))
                : string.Join(", ", type.GetTypeInfo().GenericTypeParameters.Select(t => typeless ? "" : t.GetSymbolDisplay(true)));
            return "<" + genericParams + ">";
        }

        // Output: <T, U>
        public static string GetGenericParameters(this MethodInfo method, bool typeless = false)
        {
            return method.IsGenericMethod
              ? "<" + string.Join(", ", method.GetGenericArguments().Select(t => typeless ? "" : t.Name)) + ">"
              : "";
        }
    }
}
