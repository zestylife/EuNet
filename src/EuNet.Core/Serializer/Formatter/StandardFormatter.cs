using System;
using System.Collections.Generic;

namespace EuNet.Core
{
    public sealed class ByteArrayFormatter : INetDataFormatter<byte[]>
    {
        public static readonly ByteArrayFormatter Instance = new ByteArrayFormatter();

        private ByteArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, byte[] value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public byte[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadByteArray();
        }
    }

    public sealed class NullableStringFormatter : INetDataFormatter<String>
    {
        public static readonly NullableStringFormatter Instance = new NullableStringFormatter();

        private NullableStringFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, string value, NetDataSerializerOptions options)
        {
            writer.Write(value);
        }

        public string Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return reader.ReadString();
        }
    }

    public sealed class NullableStringArrayFormatter : INetDataFormatter<String[]>
    {
        public static readonly NullableStringArrayFormatter Instance = new NullableStringArrayFormatter();

        private NullableStringArrayFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, String[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write((int)value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }

        public String[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return null;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<String>();
            }

            var array = new String[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadString();
            }

            return array;
        }
    }

    public sealed class KeyValuePairFormatter<TKey, TValue> : INetDataFormatter<KeyValuePair<TKey, TValue>>
    {
        public void Serialize(NetDataWriter writer, KeyValuePair<TKey, TValue> value, NetDataSerializerOptions options)
        {
            writer.Write((byte)2);
            INetDataFormatterResolver resolver = options.Resolver;
            resolver.GetFormatter<TKey>().Serialize(writer, value.Key, options);
            resolver.GetFormatter<TValue>().Serialize(writer, value.Value, options);
            return;
        }

        public KeyValuePair<TKey, TValue> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            var count = reader.ReadByte();

            if (count != 2)
            {
                throw new NetDataSerializationException("Invalid KeyValuePair format.");
            }

            INetDataFormatterResolver resolver = options.Resolver;

            TKey key = resolver.GetFormatter<TKey>().Deserialize(reader, options);
            TValue value = resolver.GetFormatter<TValue>().Deserialize(reader, options);
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}
