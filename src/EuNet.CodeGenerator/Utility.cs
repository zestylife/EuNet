using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EuNet.Core;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine;

namespace CodeGenerator
{
    public static class Utility
    {
        public static bool IsRpcInterface(Type type)
        {
            return type.IsInterface &&
                   type.GetInterfaces().Any(i => i.FullName == "EuNet.Rpc.IRpc");
        }

        public static bool IsViewRpcInterface(Type type)
        {
            return type.IsInterface &&
                   type.GetInterfaces().Any(i => i.FullName == "EuNet.Rpc.IViewRpc");
        }

        public static bool IsSessionInterface(Type type)
        {
            return type.FullName == "EuNet.Core.ISession";
            /*
            return type.IsInterface &&
                   type.GetInterfaces().Any(i => i.FullName == "EuNet.Core.ISession");
            */
        }

        public static bool IsNetDataObjectAttribute(Type type)
        {
            TypeInfo ti = type.GetTypeInfo();
            NetDataObjectAttribute attr = ti.GetCustomAttribute<NetDataObjectAttribute>();
            if (attr != null)
                return true;

            return false;
        }

        public static bool IsBasicSerializeType(Type type)
        {
            if (type == typeof(string) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(byte) ||
                type == typeof(bool) ||
                type == typeof(char) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(Vector2) ||
                type == typeof(Vector3) ||
                type == typeof(Vector4) ||
                type == typeof(Quaternion) ||
                type == typeof(Color) ||
                type == typeof(Bounds) ||
                type == typeof(Rect) ||
                type == typeof(Keyframe) ||
                type == typeof(AnimationCurve) ||
                type == typeof(Matrix4x4) ||
                type == typeof(GradientColorKey) ||
                type == typeof(GradientAlphaKey) ||
                type == typeof(Gradient) ||
                type == typeof(Color32) ||
                type == typeof(RectOffset) ||
                type == typeof(LayerMask) ||
                type == typeof(Vector2Int) ||
                type == typeof(Vector3Int) ||
                type == typeof(RangeInt) ||
                type == typeof(RectInt) ||
                type == typeof(BoundsInt))
                return true;

            return false;
        }

        public static string GetRpcClassName(Type type)
        {
            return type.GetPureName().Substring(1);
        }

        public static string GetRpcServiceClassName(Type type)
        {
            return type.GetPureName().Substring(1) + "ServiceAbstract";
        }

        public static string GetRpcServiceSessionClassName(Type type)
        {
            return type.GetPureName().Substring(1) + "ServiceSession";
        }

        public static string GetViewRpcServiceClassName(Type type)
        {
            return type.GetPureName().Substring(1) + "ServiceBehaviour";
        }

        public static string GetViewRpcServiceViewClassName(Type type)
        {
            return type.GetPureName().Substring(1) + "ServiceView";
        }

        public static string GetNoReplyInterfaceName(Type type)
        {
            return type.GetPureName() + "_NoReply";
        }

        public static string GetHandlerInterfaceName(Type type)
        {
            return type.GetPureName() + "_Handler";
        }

        public static string GetInterfaceEnumName(Type type)
        {
            return type.GetPureName() + "_Enum";
        }

        public static string GetFormatterClassName(Type type)
        {
            return type.GetPureName() + "Formatter";
        }

        public static string GetParameterAssignment(ParameterInfo pi)
        {
            return $"{pi.Name} = {pi.Name}";
        }

        public static IEnumerable<string> GetReachableMemebers(Type type, Func<Type, bool> filter)
        {
            // itself

            if (filter(type))
                yield return "";

            // members

            if (type.IsPrimitive)
                yield break;

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (filter(field.FieldType))
                    yield return field.Name;
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (filter(property.PropertyType) && property.GetGetMethod(false) != null)
                    yield return property.Name;
            }
        }

        public static int GetMethodHashCode(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0;

            uint hash = 5381;

            var len = name.Length;
            for (var i = 0; i < len; i++)
                hash = ((hash << 5) + hash) + name[i];

            var ihash = (int)(hash & 0x7FFFFFFF);
            var phash = ihash == 0 ? 1 : ihash;
            var result = (phash < 256)
                       ? phash = phash * 131
                       : phash;

            return result < 0 ? result : -result;
        }

        public static string GetWriteMethod(ParameterInfo paramInfo)
        {
            var type = paramInfo.ParameterType;
            if (IsSessionInterface(type))
                return string.Empty;

            return GetWriteMethod(type, paramInfo.Name);
        }

        public static string GetWriteMethod(Type type, string name)
        {
            if (IsBasicSerializeType(type) ||
                type == typeof(string[]) ||
                type == typeof(long[]) ||
                type == typeof(ulong[]) ||
                type == typeof(int[]) ||
                type == typeof(uint[]) ||
                type == typeof(short[]) ||
                type == typeof(ushort[]) ||
                type == typeof(byte[]) ||
                type == typeof(bool[]) ||
                type == typeof(char[]) ||
                type == typeof(float[]) ||
                type == typeof(double[]))
                return $"_writer_.Write({name});";

            if (type.GetInterfaces().Any(i => i.FullName == "EuNet.Core.INetSerializable"))
            {
                return $"_writer_.Write<{type.GetSymbolDisplay(true)}>({name});";
            }

            return $"NetDataSerializer.Serialize<{type.GetSymbolDisplay(true)}>(_writer_, {name});";
        }

        public static string GetReaderMethod(ParameterInfo paramInfo)
        {
            var type = paramInfo.ParameterType;
            if (IsSessionInterface(type))
                return string.Empty;

            return $"var {paramInfo.Name} = {GetReaderMethod(type)}";
        }

        public static string GetReaderMethod(Type type)
        {
            if (IsSessionInterface(type))
                return string.Empty;

            string value = string.Empty;

            if (IsBasicSerializeType(type))
                value = type.Name;
            else if (type == typeof(string[]))
                value = "StringArray";
            else if (type == typeof(long[]))
                value = "Int64Array";
            else if (type == typeof(ulong[]))
                value = "UInt64Array";
            else if (type == typeof(int[]))
                value = "Int32Array";
            else if (type == typeof(uint[]))
                value = "UInt32Array";
            else if (type == typeof(short[]))
                value = "Int16Array";
            else if (type == typeof(ushort[]))
                value = "UInt16Array";
            else if (type == typeof(byte[]))
                value = "ByteArray";
            else if (type == typeof(bool[]))
                value = "BooleanArray";
            else if (type == typeof(char[]))
                value = "CharArray";
            else if (type == typeof(float[]))
                value = "FloatArray";
            else if (type == typeof(double[]))
                value = "DoubleArray";
            else if (type.GetInterfaces().Any(i => i.FullName == "EuNet.Core.INetSerializable"))
                value = $"<{type.GetSymbolDisplay(true)}>";

            if (value.Length > 0)
                value = $"_reader_.Read{value}();";
            else value = $"NetDataSerializer.Deserialize<{type.GetSymbolDisplay(true)}>(_reader_);";

            return value;
        }
    }
}
