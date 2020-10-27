using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeWriter;
using EuNet.Core;

namespace CodeGenerator
{
    public class FormatterCodeGenerator
    {
        public class SerializeMember
        {
            public PropertyInfo PropertyInfo;
            public FieldInfo FieldInfo;
            public int Key;
            public bool IsReadable;
            public bool IsWritable;

            public string Name
            {
                get
                {
                    if (PropertyInfo != null)
                        return PropertyInfo.Name;

                    return FieldInfo.Name;
                }
            }

            public Type Type
            {
                get
                {
                    if (PropertyInfo != null)
                        return PropertyInfo.PropertyType;

                    return FieldInfo.FieldType;
                }
            }
        };

        public Options Options { get; set; }

        public void GenerateCode(Type type, Dictionary<string, string> formatterMap, CodeWriter.CodeWriter w)
        {
            Console.WriteLine("GenerateCode: " + type.GetSymbolDisplay(true));

            w._($"#region {type.GetSymbolDisplay(true)}");
            w._();

            var namespaceHandle = (string.IsNullOrEmpty(type.Namespace) == false)
                ? w.B($"namespace {type.Namespace}")
                : null;

            GenerateFormatterCode(type, formatterMap, w);

            namespaceHandle?.Dispose();

            w._();
            w._($"#endregion");
        }

        private void GenerateFormatterCode(Type type, Dictionary<string, string> formatterMap, CodeWriter.CodeWriter w)
        {
            bool allowPrivate = false;
            Dictionary<int, SerializeMember> members = new Dictionary<int, SerializeMember>();

            foreach (PropertyInfo item in GetAllProperties(type))
            {
                if (item.GetCustomAttribute<IgnoreMemberAttribute>(true) != null)
                {
                    continue;
                }

                if (item.IsIndexer())
                {
                    continue;
                }

                MethodInfo getMethod = item.GetGetMethod(true);
                MethodInfo setMethod = item.GetSetMethod(true);

                int key = Utility.GetMethodHashCode(item.Name);

                var member = new SerializeMember
                {
                    PropertyInfo = item,
                    Key = key,
                    IsReadable = (getMethod != null) && (allowPrivate || getMethod.IsPublic) && !getMethod.IsStatic,
                    IsWritable = (setMethod != null) && (allowPrivate || setMethod.IsPublic) && !setMethod.IsStatic,
                };

                if (!member.IsReadable || !member.IsWritable)
                {
                    continue;
                }

                if(members.ContainsKey(key))
                    throw new Exception("key is duplicated, all members key must be unique." + " type: " + type.FullName + " member:" + item.Name);

                members.Add(key, member);
            }

            foreach (FieldInfo item in GetAllFields(type))
            {
                if (item.GetCustomAttribute<IgnoreMemberAttribute>(true) != null)
                {
                    continue;
                }

                if (item.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>(true) != null)
                {
                    continue;
                }

                if (item.IsStatic)
                {
                    continue;
                }

                int key = Utility.GetMethodHashCode(item.Name);

                var member = new SerializeMember
                {
                    FieldInfo = item,
                    Key = key,
                    IsReadable = allowPrivate || item.IsPublic,
                    IsWritable = allowPrivate || (item.IsPublic && !item.IsInitOnly),
                };

                if (!member.IsReadable || !member.IsWritable)
                {
                    continue;
                }

                if (members.ContainsKey(key))
                    throw new Exception("key is duplicated, all members key must be unique." + " type: " + type.FullName + " member:" + item.Name);

                members.Add(key, member);
            }

            var memberArray = members.Values.OrderBy(x => x.Key).ToArray();

            var formatterClassName = Utility.GetFormatterClassName(type);
            var formatterClassGenericName = formatterClassName + type.GetGenericParameters();
            var className = type.GetPureName();
            var classGenericName = className + type.GetGenericParameters();

            using (w.B($"public sealed class {formatterClassGenericName} : INetDataFormatter<{classGenericName}>"))
            {
                if (type.IsGenericType == false)
                {
                    w._($"public static readonly {formatterClassGenericName} Instance = new {formatterClassGenericName}();");
                    w._();
                }

                // Constructor
                /*
                using (w.B($"private {formatterClassName}()"))
                {
                }
                */

                // Serializer

                using (w.B($"public void Serialize(NetDataWriter _writer_, {classGenericName} _value_, NetDataSerializerOptions options)"))
                {
                    foreach (var member in memberArray)
                    {
                        w._(Utility.GetWriteMethod(member.Type, $"_value_.{member.Name}"));
                    }
                }

                using (w.B($"public {classGenericName} Deserialize(NetDataReader _reader_, NetDataSerializerOptions options)"))
                {
                    foreach (var member in memberArray)
                    {
                        w._($"var __{member.Name} = {Utility.GetReaderMethod(member.Type)}");
                    }
                    w._();

                    using (w.i($"return new {classGenericName}() {{", "};"))
                    {
                        foreach (var member in memberArray)
                        {
                            w._($"{member.Name} = __{member.Name},");
                        }
                    }
                }
            }

            if(type.IsGenericType)
            {
                formatterMap.Add($"typeof({className + type.GetGenericParameters(true)})", $"typeof({formatterClassName + type.GetGenericParameters(true)})");
            }
            else
            {
                formatterMap.Add($"typeof({className})", $"{formatterClassName}.Instance");
            }
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            if (type.BaseType is object)
            {
                foreach (var item in GetAllProperties(type.BaseType))
                {
                    yield return item;
                }
            }

            // with declared only
            foreach (var item in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                yield return item;
            }
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            if (type.BaseType is object)
            {
                foreach (var item in GetAllFields(type.BaseType))
                {
                    yield return item;
                }
            }

            // with declared only
            foreach (var item in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                yield return item;
            }
        }
    }
}
