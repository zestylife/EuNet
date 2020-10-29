// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/* THIS (.cs) FILE IS GENERATED. DO NOT CHANGE IT.
 * CHANGE THE .tt FILE INSTEAD. */

using System;

#pragma warning disable SA1649 // File name should match first type name

namespace EuNet.Core
{
    public sealed class Int16Formatter : INetDataFormatter<Int16>
    {
        public static readonly Int16Formatter Instance = new Int16Formatter();

        private Int16Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, Int16 value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public Int16 Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadInt16();
        }
    }

    public sealed class NullableInt16Formatter : INetDataFormatter<Int16?>
    {
        public static readonly NullableInt16Formatter Instance = new NullableInt16Formatter();

        private NullableInt16Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, Int16? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public Int16? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadInt16();
            }
        }
    }

    public sealed class Int16ArrayFormatter : INetDataFormatter<Int16[]>
    {
        public static readonly Int16ArrayFormatter Instance = new Int16ArrayFormatter();

        private Int16ArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Int16[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public Int16[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<Int16>();
            }

            var array = new Int16[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadInt16();
            }

            return array;
        }
    }

    public sealed class Int32Formatter : INetDataFormatter<Int32>
    {
        public static readonly Int32Formatter Instance = new Int32Formatter();

        private Int32Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, Int32 value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public Int32 Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadInt32();
        }
    }

    public sealed class NullableInt32Formatter : INetDataFormatter<Int32?>
    {
        public static readonly NullableInt32Formatter Instance = new NullableInt32Formatter();

        private NullableInt32Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, Int32? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public Int32? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadInt32();
            }
        }
    }

    public sealed class Int32ArrayFormatter : INetDataFormatter<Int32[]>
    {
        public static readonly Int32ArrayFormatter Instance = new Int32ArrayFormatter();

        private Int32ArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Int32[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public Int32[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<Int32>();
            }

            var array = new Int32[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadInt32();
            }

            return array;
        }
    }

    public sealed class Int64Formatter : INetDataFormatter<Int64>
    {
        public static readonly Int64Formatter Instance = new Int64Formatter();

        private Int64Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, Int64 value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public Int64 Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadInt64();
        }
    }

    public sealed class NullableInt64Formatter : INetDataFormatter<Int64?>
    {
        public static readonly NullableInt64Formatter Instance = new NullableInt64Formatter();

        private NullableInt64Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, Int64? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public Int64? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadInt64();
            }
        }
    }

    public sealed class Int64ArrayFormatter : INetDataFormatter<Int64[]>
    {
        public static readonly Int64ArrayFormatter Instance = new Int64ArrayFormatter();

        private Int64ArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Int64[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public Int64[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<Int64>();
            }

            var array = new Int64[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadInt64();
            }

            return array;
        }
    }

    public sealed class UInt16Formatter : INetDataFormatter<UInt16>
    {
        public static readonly UInt16Formatter Instance = new UInt16Formatter();

        private UInt16Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, UInt16 value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public UInt16 Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadUInt16();
        }
    }

    public sealed class NullableUInt16Formatter : INetDataFormatter<UInt16?>
    {
        public static readonly NullableUInt16Formatter Instance = new NullableUInt16Formatter();

        private NullableUInt16Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, UInt16? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public UInt16? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadUInt16();
            }
        }
    }

    public sealed class UInt16ArrayFormatter : INetDataFormatter<UInt16[]>
    {
        public static readonly UInt16ArrayFormatter Instance = new UInt16ArrayFormatter();

        private UInt16ArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, UInt16[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public UInt16[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<UInt16>();
            }

            var array = new UInt16[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadUInt16();
            }

            return array;
        }
    }

    public sealed class UInt32Formatter : INetDataFormatter<UInt32>
    {
        public static readonly UInt32Formatter Instance = new UInt32Formatter();

        private UInt32Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, UInt32 value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public UInt32 Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadUInt32();
        }
    }

    public sealed class NullableUInt32Formatter : INetDataFormatter<UInt32?>
    {
        public static readonly NullableUInt32Formatter Instance = new NullableUInt32Formatter();

        private NullableUInt32Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, UInt32? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public UInt32? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadUInt32();
            }
        }
    }

    public sealed class UInt32ArrayFormatter : INetDataFormatter<UInt32[]>
    {
        public static readonly UInt32ArrayFormatter Instance = new UInt32ArrayFormatter();

        private UInt32ArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, UInt32[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public UInt32[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<UInt32>();
            }

            var array = new UInt32[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadUInt32();
            }

            return array;
        }
    }

    public sealed class UInt64Formatter : INetDataFormatter<UInt64>
    {
        public static readonly UInt64Formatter Instance = new UInt64Formatter();

        private UInt64Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, UInt64 value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public UInt64 Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadUInt64();
        }
    }

    public sealed class NullableUInt64Formatter : INetDataFormatter<UInt64?>
    {
        public static readonly NullableUInt64Formatter Instance = new NullableUInt64Formatter();

        private NullableUInt64Formatter()
        {
        }

        public void Serialize(NetDataWriter writer, UInt64? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public UInt64? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadUInt64();
            }
        }
    }

    public sealed class UInt64ArrayFormatter : INetDataFormatter<UInt64[]>
    {
        public static readonly UInt64ArrayFormatter Instance = new UInt64ArrayFormatter();

        private UInt64ArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, UInt64[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public UInt64[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<UInt64>();
            }

            var array = new UInt64[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadUInt64();
            }

            return array;
        }
    }

    public sealed class SingleFormatter : INetDataFormatter<Single>
    {
        public static readonly SingleFormatter Instance = new SingleFormatter();

        private SingleFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Single value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public Single Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadSingle();
        }
    }

    public sealed class NullableSingleFormatter : INetDataFormatter<Single?>
    {
        public static readonly NullableSingleFormatter Instance = new NullableSingleFormatter();

        private NullableSingleFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Single? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public Single? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadSingle();
            }
        }
    }

    public sealed class SingleArrayFormatter : INetDataFormatter<Single[]>
    {
        public static readonly SingleArrayFormatter Instance = new SingleArrayFormatter();

        private SingleArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Single[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public Single[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<Single>();
            }

            var array = new Single[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadSingle();
            }

            return array;
        }
    }

    public sealed class DoubleFormatter : INetDataFormatter<Double>
    {
        public static readonly DoubleFormatter Instance = new DoubleFormatter();

        private DoubleFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Double value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public Double Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadDouble();
        }
    }

    public sealed class NullableDoubleFormatter : INetDataFormatter<Double?>
    {
        public static readonly NullableDoubleFormatter Instance = new NullableDoubleFormatter();

        private NullableDoubleFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Double? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public Double? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadDouble();
            }
        }
    }

    public sealed class DoubleArrayFormatter : INetDataFormatter<Double[]>
    {
        public static readonly DoubleArrayFormatter Instance = new DoubleArrayFormatter();

        private DoubleArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Double[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public Double[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<Double>();
            }

            var array = new Double[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadDouble();
            }

            return array;
        }
    }

    public sealed class BooleanFormatter : INetDataFormatter<Boolean>
    {
        public static readonly BooleanFormatter Instance = new BooleanFormatter();

        private BooleanFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Boolean value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public Boolean Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadBoolean();
        }
    }

    public sealed class NullableBooleanFormatter : INetDataFormatter<Boolean?>
    {
        public static readonly NullableBooleanFormatter Instance = new NullableBooleanFormatter();

        private NullableBooleanFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Boolean? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public Boolean? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadBoolean();
            }
        }
    }

    public sealed class BooleanArrayFormatter : INetDataFormatter<Boolean[]>
    {
        public static readonly BooleanArrayFormatter Instance = new BooleanArrayFormatter();

        private BooleanArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Boolean[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public Boolean[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<Boolean>();
            }

            var array = new Boolean[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadBoolean();
            }

            return array;
        }
    }

    public sealed class ByteFormatter : INetDataFormatter<Byte>
    {
        public static readonly ByteFormatter Instance = new ByteFormatter();

        private ByteFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Byte value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public Byte Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadByte();
        }
    }

    public sealed class NullableByteFormatter : INetDataFormatter<Byte?>
    {
        public static readonly NullableByteFormatter Instance = new NullableByteFormatter();

        private NullableByteFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Byte? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public Byte? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadByte();
            }
        }
    }

    public sealed class SByteFormatter : INetDataFormatter<SByte>
    {
        public static readonly SByteFormatter Instance = new SByteFormatter();

        private SByteFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, SByte value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public SByte Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadSByte();
        }
    }

    public sealed class NullableSByteFormatter : INetDataFormatter<SByte?>
    {
        public static readonly NullableSByteFormatter Instance = new NullableSByteFormatter();

        private NullableSByteFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, SByte? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public SByte? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadSByte();
            }
        }
    }

    public sealed class SByteArrayFormatter : INetDataFormatter<SByte[]>
    {
        public static readonly SByteArrayFormatter Instance = new SByteArrayFormatter();

        private SByteArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, SByte[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public SByte[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<SByte>();
            }

            var array = new SByte[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadSByte();
            }

            return array;
        }
    }

    public sealed class CharFormatter : INetDataFormatter<Char>
    {
        public static readonly CharFormatter Instance = new CharFormatter();

        private CharFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Char value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public Char Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadChar();
        }
    }

    public sealed class NullableCharFormatter : INetDataFormatter<Char?>
    {
        public static readonly NullableCharFormatter Instance = new NullableCharFormatter();

        private NullableCharFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Char? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public Char? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadChar();
            }
        }
    }

    public sealed class CharArrayFormatter : INetDataFormatter<Char[]>
    {
        public static readonly CharArrayFormatter Instance = new CharArrayFormatter();

        private CharArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Char[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public Char[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<Char>();
            }

            var array = new Char[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadChar();
            }

            return array;
        }
    }

    public sealed class DateTimeFormatter : INetDataFormatter<DateTime>
    {
        public static readonly DateTimeFormatter Instance = new DateTimeFormatter();

        private DateTimeFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, DateTime value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public DateTime Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadDateTime();
        }
    }

    public sealed class NullableDateTimeFormatter : INetDataFormatter<DateTime?>
    {
        public static readonly NullableDateTimeFormatter Instance = new NullableDateTimeFormatter();

        private NullableDateTimeFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, DateTime? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public DateTime? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadDateTime();
            }
        }
    }

    public sealed class DateTimeArrayFormatter : INetDataFormatter<DateTime[]>
    {
        public static readonly DateTimeArrayFormatter Instance = new DateTimeArrayFormatter();

        private DateTimeArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, DateTime[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public DateTime[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<DateTime>();
            }

            var array = new DateTime[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadDateTime();
            }

            return array;
        }
    }

    public sealed class TimeSpanFormatter : INetDataFormatter<TimeSpan>
    {
        public static readonly TimeSpanFormatter Instance = new TimeSpanFormatter();

        private TimeSpanFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, TimeSpan value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public TimeSpan Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadTimeSpan();
        }
    }

    public sealed class NullableTimeSpanFormatter : INetDataFormatter<TimeSpan?>
    {
        public static readonly NullableTimeSpanFormatter Instance = new NullableTimeSpanFormatter();

        private NullableTimeSpanFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, TimeSpan? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public TimeSpan? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadTimeSpan();
            }
        }
    }

    public sealed class TimeSpanArrayFormatter : INetDataFormatter<TimeSpan[]>
    {
        public static readonly TimeSpanArrayFormatter Instance = new TimeSpanArrayFormatter();

        private TimeSpanArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, TimeSpan[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public TimeSpan[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<TimeSpan>();
            }

            var array = new TimeSpan[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadTimeSpan();
            }

            return array;
        }
    }

    public sealed class GuidFormatter : INetDataFormatter<Guid>
    {
        public static readonly GuidFormatter Instance = new GuidFormatter();

        private GuidFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Guid value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public Guid Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadGuid();
        }
    }

    public sealed class NullableGuidFormatter : INetDataFormatter<Guid?>
    {
        public static readonly NullableGuidFormatter Instance = new NullableGuidFormatter();

        private NullableGuidFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Guid? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
        }

        public Guid? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                return reader.ReadGuid();
            }
        }
    }

    public sealed class GuidArrayFormatter : INetDataFormatter<Guid[]>
    {
        public static readonly GuidArrayFormatter Instance = new GuidArrayFormatter();

        private GuidArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, Guid[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public Guid[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<Guid>();
            }

            var array = new Guid[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadGuid();
            }

            return array;
        }
    }
}
