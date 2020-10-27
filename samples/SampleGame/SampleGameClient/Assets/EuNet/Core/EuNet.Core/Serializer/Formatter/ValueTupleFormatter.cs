// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/* THIS (.cs) FILE IS GENERATED. DO NOT CHANGE IT.
 * CHANGE THE .tt FILE INSTEAD. */

using System;

#pragma warning disable SA1649 // File name should match first type name

namespace EuNet.Core
{
    public sealed class ValueTupleFormatter<T1> : INetDataFormatter<ValueTuple<T1>>
    {
        public void Serialize(NetDataWriter writer, ValueTuple<T1> value, NetDataSerializerOptions options)
        {
            writer.Write((byte)1);

            INetDataFormatterResolver resolver = options.Resolver;
            resolver.GetFormatter<T1>().Serialize(writer, value.Item1, options);
        }

        public ValueTuple<T1> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            var count = reader.ReadByte();
            if (count != 1)
            {
                throw new NetDataSerializationException("Invalid ValueTuple count");
            }

            INetDataFormatterResolver resolver = options.Resolver;
            
            T1 item1 = resolver.GetFormatter<T1>().Deserialize(reader, options);
            return new ValueTuple<T1>(item1);
        }
    }

    public sealed class ValueTupleFormatter<T1, T2> : INetDataFormatter<ValueTuple<T1, T2>>
    {
        public void Serialize(NetDataWriter writer, ValueTuple<T1, T2> value, NetDataSerializerOptions options)
        {
            writer.Write((byte)2);

            INetDataFormatterResolver resolver = options.Resolver;
            resolver.GetFormatter<T1>().Serialize(writer, value.Item1, options);
            resolver.GetFormatter<T2>().Serialize(writer, value.Item2, options);
        }

        public ValueTuple<T1, T2> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            var count = reader.ReadByte();
            if (count != 2)
            {
                throw new NetDataSerializationException("Invalid ValueTuple count");
            }

            INetDataFormatterResolver resolver = options.Resolver;
            
            T1 item1 = resolver.GetFormatter<T1>().Deserialize(reader, options);
            T2 item2 = resolver.GetFormatter<T2>().Deserialize(reader, options);
            return new ValueTuple<T1, T2>(item1, item2);
        }
    }

    public sealed class ValueTupleFormatter<T1, T2, T3> : INetDataFormatter<ValueTuple<T1, T2, T3>>
    {
        public void Serialize(NetDataWriter writer, ValueTuple<T1, T2, T3> value, NetDataSerializerOptions options)
        {
            writer.Write((byte)3);

            INetDataFormatterResolver resolver = options.Resolver;
            resolver.GetFormatter<T1>().Serialize(writer, value.Item1, options);
            resolver.GetFormatter<T2>().Serialize(writer, value.Item2, options);
            resolver.GetFormatter<T3>().Serialize(writer, value.Item3, options);
        }

        public ValueTuple<T1, T2, T3> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            var count = reader.ReadByte();
            if (count != 3)
            {
                throw new NetDataSerializationException("Invalid ValueTuple count");
            }

            INetDataFormatterResolver resolver = options.Resolver;
            
            T1 item1 = resolver.GetFormatter<T1>().Deserialize(reader, options);
            T2 item2 = resolver.GetFormatter<T2>().Deserialize(reader, options);
            T3 item3 = resolver.GetFormatter<T3>().Deserialize(reader, options);
            return new ValueTuple<T1, T2, T3>(item1, item2, item3);
        }
    }

    public sealed class ValueTupleFormatter<T1, T2, T3, T4> : INetDataFormatter<ValueTuple<T1, T2, T3, T4>>
    {
        public void Serialize(NetDataWriter writer, ValueTuple<T1, T2, T3, T4> value, NetDataSerializerOptions options)
        {
            writer.Write((byte)4);

            INetDataFormatterResolver resolver = options.Resolver;
            resolver.GetFormatter<T1>().Serialize(writer, value.Item1, options);
            resolver.GetFormatter<T2>().Serialize(writer, value.Item2, options);
            resolver.GetFormatter<T3>().Serialize(writer, value.Item3, options);
            resolver.GetFormatter<T4>().Serialize(writer, value.Item4, options);
        }

        public ValueTuple<T1, T2, T3, T4> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            var count = reader.ReadByte();
            if (count != 4)
            {
                throw new NetDataSerializationException("Invalid ValueTuple count");
            }

            INetDataFormatterResolver resolver = options.Resolver;
            
            T1 item1 = resolver.GetFormatter<T1>().Deserialize(reader, options);
            T2 item2 = resolver.GetFormatter<T2>().Deserialize(reader, options);
            T3 item3 = resolver.GetFormatter<T3>().Deserialize(reader, options);
            T4 item4 = resolver.GetFormatter<T4>().Deserialize(reader, options);
            return new ValueTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }

    public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5> : INetDataFormatter<ValueTuple<T1, T2, T3, T4, T5>>
    {
        public void Serialize(NetDataWriter writer, ValueTuple<T1, T2, T3, T4, T5> value, NetDataSerializerOptions options)
        {
            writer.Write((byte)5);

            INetDataFormatterResolver resolver = options.Resolver;
            resolver.GetFormatter<T1>().Serialize(writer, value.Item1, options);
            resolver.GetFormatter<T2>().Serialize(writer, value.Item2, options);
            resolver.GetFormatter<T3>().Serialize(writer, value.Item3, options);
            resolver.GetFormatter<T4>().Serialize(writer, value.Item4, options);
            resolver.GetFormatter<T5>().Serialize(writer, value.Item5, options);
        }

        public ValueTuple<T1, T2, T3, T4, T5> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            var count = reader.ReadByte();
            if (count != 5)
            {
                throw new NetDataSerializationException("Invalid ValueTuple count");
            }

            INetDataFormatterResolver resolver = options.Resolver;
            
            T1 item1 = resolver.GetFormatter<T1>().Deserialize(reader, options);
            T2 item2 = resolver.GetFormatter<T2>().Deserialize(reader, options);
            T3 item3 = resolver.GetFormatter<T3>().Deserialize(reader, options);
            T4 item4 = resolver.GetFormatter<T4>().Deserialize(reader, options);
            T5 item5 = resolver.GetFormatter<T5>().Deserialize(reader, options);
            return new ValueTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }
    }

    public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6> : INetDataFormatter<ValueTuple<T1, T2, T3, T4, T5, T6>>
    {
        public void Serialize(NetDataWriter writer, ValueTuple<T1, T2, T3, T4, T5, T6> value, NetDataSerializerOptions options)
        {
            writer.Write((byte)6);

            INetDataFormatterResolver resolver = options.Resolver;
            resolver.GetFormatter<T1>().Serialize(writer, value.Item1, options);
            resolver.GetFormatter<T2>().Serialize(writer, value.Item2, options);
            resolver.GetFormatter<T3>().Serialize(writer, value.Item3, options);
            resolver.GetFormatter<T4>().Serialize(writer, value.Item4, options);
            resolver.GetFormatter<T5>().Serialize(writer, value.Item5, options);
            resolver.GetFormatter<T6>().Serialize(writer, value.Item6, options);
        }

        public ValueTuple<T1, T2, T3, T4, T5, T6> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            var count = reader.ReadByte();
            if (count != 6)
            {
                throw new NetDataSerializationException("Invalid ValueTuple count");
            }

            INetDataFormatterResolver resolver = options.Resolver;
            
            T1 item1 = resolver.GetFormatter<T1>().Deserialize(reader, options);
            T2 item2 = resolver.GetFormatter<T2>().Deserialize(reader, options);
            T3 item3 = resolver.GetFormatter<T3>().Deserialize(reader, options);
            T4 item4 = resolver.GetFormatter<T4>().Deserialize(reader, options);
            T5 item5 = resolver.GetFormatter<T5>().Deserialize(reader, options);
            T6 item6 = resolver.GetFormatter<T6>().Deserialize(reader, options);
            return new ValueTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }
    }

    public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7> : INetDataFormatter<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        public void Serialize(NetDataWriter writer, ValueTuple<T1, T2, T3, T4, T5, T6, T7> value, NetDataSerializerOptions options)
        {
            writer.Write((byte)7);

            INetDataFormatterResolver resolver = options.Resolver;
            resolver.GetFormatter<T1>().Serialize(writer, value.Item1, options);
            resolver.GetFormatter<T2>().Serialize(writer, value.Item2, options);
            resolver.GetFormatter<T3>().Serialize(writer, value.Item3, options);
            resolver.GetFormatter<T4>().Serialize(writer, value.Item4, options);
            resolver.GetFormatter<T5>().Serialize(writer, value.Item5, options);
            resolver.GetFormatter<T6>().Serialize(writer, value.Item6, options);
            resolver.GetFormatter<T7>().Serialize(writer, value.Item7, options);
        }

        public ValueTuple<T1, T2, T3, T4, T5, T6, T7> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            var count = reader.ReadByte();
            if (count != 7)
            {
                throw new NetDataSerializationException("Invalid ValueTuple count");
            }

            INetDataFormatterResolver resolver = options.Resolver;
            
            T1 item1 = resolver.GetFormatter<T1>().Deserialize(reader, options);
            T2 item2 = resolver.GetFormatter<T2>().Deserialize(reader, options);
            T3 item3 = resolver.GetFormatter<T3>().Deserialize(reader, options);
            T4 item4 = resolver.GetFormatter<T4>().Deserialize(reader, options);
            T5 item5 = resolver.GetFormatter<T5>().Deserialize(reader, options);
            T6 item6 = resolver.GetFormatter<T6>().Deserialize(reader, options);
            T7 item7 = resolver.GetFormatter<T7>().Deserialize(reader, options);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }
    }

    public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : INetDataFormatter<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>
        where TRest : struct
    {
        public void Serialize(NetDataWriter writer, ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> value, NetDataSerializerOptions options)
        {
            writer.Write((byte)8);

            INetDataFormatterResolver resolver = options.Resolver;
            resolver.GetFormatter<T1>().Serialize(writer, value.Item1, options);
            resolver.GetFormatter<T2>().Serialize(writer, value.Item2, options);
            resolver.GetFormatter<T3>().Serialize(writer, value.Item3, options);
            resolver.GetFormatter<T4>().Serialize(writer, value.Item4, options);
            resolver.GetFormatter<T5>().Serialize(writer, value.Item5, options);
            resolver.GetFormatter<T6>().Serialize(writer, value.Item6, options);
            resolver.GetFormatter<T7>().Serialize(writer, value.Item7, options);
            resolver.GetFormatter<TRest>().Serialize(writer, value.Rest, options);
        }

        public ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            var count = reader.ReadByte();
            if (count != 8)
            {
                throw new NetDataSerializationException("Invalid ValueTuple count");
            }

            INetDataFormatterResolver resolver = options.Resolver;
            
            T1 item1 = resolver.GetFormatter<T1>().Deserialize(reader, options);
            T2 item2 = resolver.GetFormatter<T2>().Deserialize(reader, options);
            T3 item3 = resolver.GetFormatter<T3>().Deserialize(reader, options);
            T4 item4 = resolver.GetFormatter<T4>().Deserialize(reader, options);
            T5 item5 = resolver.GetFormatter<T5>().Deserialize(reader, options);
            T6 item6 = resolver.GetFormatter<T6>().Deserialize(reader, options);
            T7 item7 = resolver.GetFormatter<T7>().Deserialize(reader, options);
            TRest item8 = resolver.GetFormatter<TRest>().Deserialize(reader, options);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(item1, item2, item3, item4, item5, item6, item7, item8);
        }
    }
}
