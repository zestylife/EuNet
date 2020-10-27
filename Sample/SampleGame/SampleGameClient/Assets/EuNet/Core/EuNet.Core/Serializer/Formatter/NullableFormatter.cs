// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable SA1649 // File name should match first type name

namespace EuNet.Core
{
    public sealed class NullableFormatter<T> : INetDataFormatter<T?>
        where T : struct
    {
        public void Serialize(NetDataWriter writer, T? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                options.Resolver.GetFormatter<T>().Serialize(writer, value.Value, options);
            }
        }

        public T? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return null;
            }
            else
            {
                return options.Resolver.GetFormatter<T>().Deserialize(reader, options);
            }
        }
    }

    public sealed class StaticNullableFormatter<T> : INetDataFormatter<T?>
        where T : struct
    {
        private readonly INetDataFormatter<T> underlyingFormatter;

        public StaticNullableFormatter(INetDataFormatter<T> underlyingFormatter)
        {
            this.underlyingFormatter = underlyingFormatter;
        }

        public void Serialize(NetDataWriter writer, T? value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                this.underlyingFormatter.Serialize(writer, value.Value, options);
            }
        }

        public T? Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return null;
            }
            else
            {
                return this.underlyingFormatter.Deserialize(reader, options);
            }
        }
    }
}
