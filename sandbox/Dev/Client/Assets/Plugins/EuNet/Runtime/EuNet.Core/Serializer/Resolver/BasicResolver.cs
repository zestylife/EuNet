using EuNet.Core.Internal;
using System;
using System.Collections.Generic;

namespace EuNet.Core
{
    public class BasicResolver : INetDataFormatterResolver
    {
        public static readonly BasicResolver Instance = new BasicResolver();

        private BasicResolver()
        {
        }

        public INetDataFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            public static readonly INetDataFormatter<T> Formatter;

            static FormatterCache()
            {
                Formatter = (INetDataFormatter<T>)BasicResolverGetFormatterHelper.GetFormatter(typeof(T));
            }
        }
    }
}

namespace EuNet.Core.Internal
{
    internal static class BasicResolverGetFormatterHelper
    {
        private static readonly Dictionary<Type, object> FormatterMap = new Dictionary<Type, object>()
        {
            // Primitive
            { typeof(Int16), Int16Formatter.Instance },
            { typeof(Int32), Int32Formatter.Instance },
            { typeof(Int64), Int64Formatter.Instance },
            { typeof(UInt16), UInt16Formatter.Instance },
            { typeof(UInt32), UInt32Formatter.Instance },
            { typeof(UInt64), UInt64Formatter.Instance },
            { typeof(Single), SingleFormatter.Instance },
            { typeof(Double), DoubleFormatter.Instance },
            { typeof(bool), BooleanFormatter.Instance },
            { typeof(byte), ByteFormatter.Instance },
            { typeof(sbyte), SByteFormatter.Instance },
            { typeof(DateTime), DateTimeFormatter.Instance },
            { typeof(char), CharFormatter.Instance },

            // Nulllable Primitive
            { typeof(Int16?), NullableInt16Formatter.Instance },
            { typeof(Int32?), NullableInt32Formatter.Instance },
            { typeof(Int64?), NullableInt64Formatter.Instance },
            { typeof(UInt16?), NullableUInt16Formatter.Instance },
            { typeof(UInt32?), NullableUInt32Formatter.Instance },
            { typeof(UInt64?), NullableUInt64Formatter.Instance },
            { typeof(Single?), NullableSingleFormatter.Instance },
            { typeof(Double?), NullableDoubleFormatter.Instance },
            { typeof(bool?), NullableBooleanFormatter.Instance },
            { typeof(byte?), NullableByteFormatter.Instance },
            { typeof(sbyte?), NullableSByteFormatter.Instance },
            { typeof(DateTime?), NullableDateTimeFormatter.Instance },
            { typeof(TimeSpan?), NullableTimeSpanFormatter.Instance },
            { typeof(char?), NullableCharFormatter.Instance },
            
            // special primitive
            { typeof(byte[]), ByteArrayFormatter.Instance },
            { typeof(string), NullableStringFormatter.Instance },

            // Nil
            //{ typeof(Nil), NilFormatter.Instance },
            //{ typeof(Nil?), NullableNilFormatter.Instance },

            // optimized primitive array formatter
            { typeof(Int16[]), Int16ArrayFormatter.Instance },
            { typeof(Int32[]), Int32ArrayFormatter.Instance },
            { typeof(Int64[]), Int64ArrayFormatter.Instance },
            { typeof(UInt16[]), UInt16ArrayFormatter.Instance },
            { typeof(UInt32[]), UInt32ArrayFormatter.Instance },
            { typeof(UInt64[]), UInt64ArrayFormatter.Instance },
            { typeof(Single[]), SingleArrayFormatter.Instance },
            { typeof(Double[]), DoubleArrayFormatter.Instance },
            { typeof(Boolean[]), BooleanArrayFormatter.Instance },
            { typeof(SByte[]), SByteArrayFormatter.Instance },
            { typeof(DateTime[]), DateTimeArrayFormatter.Instance },
            { typeof(Char[]), CharArrayFormatter.Instance },
            { typeof(string[]), NullableStringArrayFormatter.Instance },

            // well known collections
            { typeof(List<Int16>), new ListFormatter<Int16>() },
            { typeof(List<Int32>), new ListFormatter<Int32>() },
            { typeof(List<Int64>), new ListFormatter<Int64>() },
            { typeof(List<UInt16>), new ListFormatter<UInt16>() },
            { typeof(List<UInt32>), new ListFormatter<UInt32>() },
            { typeof(List<UInt64>), new ListFormatter<UInt64>() },
            { typeof(List<Single>), new ListFormatter<Single>() },
            { typeof(List<Double>), new ListFormatter<Double>() },
            { typeof(List<Boolean>), new ListFormatter<Boolean>() },
            { typeof(List<byte>), new ListFormatter<byte>() },
            { typeof(List<SByte>), new ListFormatter<SByte>() },
            { typeof(List<DateTime>), new ListFormatter<DateTime>() },
            { typeof(List<Char>), new ListFormatter<Char>() },
            { typeof(List<string>), new ListFormatter<string>() },

            { typeof(object[]), new ArrayFormatter<object>() },
            { typeof(List<object>), new ListFormatter<object>() },

            { typeof(ArraySegment<byte>), ByteArraySegmentFormatter.Instance },
            //{ typeof(ArraySegment<byte>?), new StaticNullableFormatter<ArraySegment<byte>>(ByteArraySegmentFormatter.Instance) },
        };

        internal static object GetFormatter(Type t)
        {
            object formatter;
            if (FormatterMap.TryGetValue(t, out formatter))
            {
                return formatter;
            }

            return null;
        }
    }
}
