// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable SA1649 // File name should match first type name

namespace EuNet.Core
{
    /* multi dimensional array serialize to [i, j, [seq]] */

    public sealed class TwoDimensionalArrayFormatter<T> : INetDataFormatter<T[,]>
    {
        private const byte ArrayLength = 3;

        public void Serialize(NetDataWriter writer, T[,] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                var i = value.GetLength(0);
                var j = value.GetLength(1);

                INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();

                writer.Write(ArrayLength);
                writer.Write(i);
                writer.Write(j);

                writer.Write((int)value.Length);
                foreach (T item in value)
                {
                    formatter.Serialize(writer, item, options);
                }
            }
        }

        public T[,] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return null;
            }
            else
            {
                INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();

                var len = reader.ReadByte();
                if (len != ArrayLength)
                {
                    throw new NetDataSerializationException("Invalid T[,] format");
                }

                var iLength = reader.ReadInt32();
                var jLength = reader.ReadInt32();
                var maxLen = reader.ReadInt32();

                var array = new T[iLength, jLength];

                var i = 0;
                var j = -1;

                for (int loop = 0; loop < maxLen; loop++)
                {
                    if (j < jLength - 1)
                    {
                        j++;
                    }
                    else
                    {
                        j = 0;
                        i++;
                    }

                    array[i, j] = formatter.Deserialize(reader, options);
                }

                return array;
            }
        }
    }

    public sealed class ThreeDimensionalArrayFormatter<T> : INetDataFormatter<T[,,]>
    {
        private const byte ArrayLength = 4;

        public void Serialize(NetDataWriter writer, T[,,] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                var i = value.GetLength(0);
                var j = value.GetLength(1);
                var k = value.GetLength(2);

                INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();

                writer.Write(ArrayLength);
                writer.Write(i);
                writer.Write(j);
                writer.Write(k);

                writer.Write((int)value.Length);
                foreach (T item in value)
                {
                    formatter.Serialize(writer, item, options);
                }
            }
        }

        public T[,,] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return null;
            }
            else
            {
                INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();

                var len = reader.ReadByte();
                if (len != ArrayLength)
                {
                    throw new NetDataSerializationException("Invalid T[,,] format");
                }

                var iLength = reader.ReadInt32();
                var jLength = reader.ReadInt32();
                var kLength = reader.ReadInt32();
                var maxLen = reader.ReadInt32();

                var array = new T[iLength, jLength, kLength];

                var i = 0;
                var j = 0;
                var k = -1;

                for (int loop = 0; loop < maxLen; loop++)
                {
                    if (k < kLength - 1)
                    {
                        k++;
                    }
                    else if (j < jLength - 1)
                    {
                        k = 0;
                        j++;
                    }
                    else
                    {
                        k = 0;
                        j = 0;
                        i++;
                    }

                    array[i, j, k] = formatter.Deserialize(reader, options);
                }

                return array;
            }
        }
    }

    public sealed class FourDimensionalArrayFormatter<T> : INetDataFormatter<T[,,,]>
    {
        private const byte ArrayLength = 5;

        public void Serialize(NetDataWriter writer, T[,,,] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                var i = value.GetLength(0);
                var j = value.GetLength(1);
                var k = value.GetLength(2);
                var l = value.GetLength(3);

                INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();

                writer.Write(ArrayLength);
                writer.Write(i);
                writer.Write(j);
                writer.Write(k);
                writer.Write(l);

                writer.Write(value.Length);
                foreach (T item in value)
                {
                    formatter.Serialize(writer, item, options);
                }
            }
        }

        public T[,,,] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return null;
            }
            else
            {
                INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();

                var len = reader.ReadByte();
                if (len != ArrayLength)
                {
                    throw new NetDataSerializationException("Invalid T[,,,] format");
                }

                var iLength = reader.ReadInt32();
                var jLength = reader.ReadInt32();
                var kLength = reader.ReadInt32();
                var lLength = reader.ReadInt32();
                var maxLen = reader.ReadInt32();
                var array = new T[iLength, jLength, kLength, lLength];

                var i = 0;
                var j = 0;
                var k = 0;
                var l = -1;

                for (int loop = 0; loop < maxLen; loop++)
                {
                    if (l < lLength - 1)
                    {
                        l++;
                    }
                    else if (k < kLength - 1)
                    {
                        l = 0;
                        k++;
                    }
                    else if (j < jLength - 1)
                    {
                        l = 0;
                        k = 0;
                        j++;
                    }
                    else
                    {
                        l = 0;
                        k = 0;
                        j = 0;
                        i++;
                    }

                    array[i, j, k, l] = formatter.Deserialize(reader, options);
                }

                return array;
            }
        }
    }
}
