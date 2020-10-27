// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

#pragma warning disable SA1649 // File name should match first type name

namespace EuNet.Core
{
    // unfortunately, can't use IDictionary<KVP> because supports IReadOnlyDictionary.
    public abstract class DictionaryFormatterBase<TKey, TValue, TIntermediate, TEnumerator, TDictionary> : INetDataFormatter<TDictionary>
        where TDictionary : IEnumerable<KeyValuePair<TKey, TValue>>
        where TEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        public void Serialize(NetDataWriter writer, TDictionary value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                INetDataFormatterResolver resolver = options.Resolver;
                INetDataFormatter<TKey> keyFormatter = resolver.GetFormatter<TKey>();
                INetDataFormatter<TValue> valueFormatter = resolver.GetFormatter<TValue>();

                int count;
                {
                    var col = value as ICollection<KeyValuePair<TKey, TValue>>;
                    if (col != null)
                    {
                        count = col.Count;
                    }
                    else
                    {
                        var col2 = value as IReadOnlyCollection<KeyValuePair<TKey, TValue>>;
                        if (col2 != null)
                        {
                            count = col2.Count;
                        }
                        else
                        {
                            throw new NetDataSerializationException("DictionaryFormatterBase's TDictionary supports only ICollection<KVP> or IReadOnlyCollection<KVP>");
                        }
                    }
                }

                writer.Write(count);

                TEnumerator e = this.GetSourceEnumerator(value);
                try
                {
                    while (e.MoveNext())
                    {
                        KeyValuePair<TKey, TValue> item = e.Current;
                        keyFormatter.Serialize(writer, item.Key, options);
                        valueFormatter.Serialize(writer, item.Value, options);
                    }
                }
                finally
                {
                    e.Dispose();
                }
            }
        }

        public TDictionary Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default(TDictionary);
            }
            else
            {
                INetDataFormatterResolver resolver = options.Resolver;
                INetDataFormatter<TKey> keyFormatter = resolver.GetFormatter<TKey>();
                INetDataFormatter<TValue> valueFormatter = resolver.GetFormatter<TValue>();

                var len = reader.ReadInt32();

                TIntermediate dict = this.Create(len, options);

                for (int i = 0; i < len; i++)
                {
                    TKey key = keyFormatter.Deserialize(reader, options);
                    TValue value = valueFormatter.Deserialize(reader, options);

                    this.Add(dict, i, key, value, options);
                }

                return this.Complete(dict);
            }
        }

        // abstraction for serialize

        // Some collections can use struct iterator, this is optimization path
        protected abstract TEnumerator GetSourceEnumerator(TDictionary source);

        // abstraction for deserialize
        protected abstract TIntermediate Create(int count, NetDataSerializerOptions options);

        protected abstract void Add(TIntermediate collection, int index, TKey key, TValue value, NetDataSerializerOptions options);

        protected abstract TDictionary Complete(TIntermediate intermediateCollection);
    }

    public abstract class DictionaryFormatterBase<TKey, TValue, TIntermediate, TDictionary> : DictionaryFormatterBase<TKey, TValue, TIntermediate, IEnumerator<KeyValuePair<TKey, TValue>>, TDictionary>
        where TDictionary : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        protected override IEnumerator<KeyValuePair<TKey, TValue>> GetSourceEnumerator(TDictionary source)
        {
            return source.GetEnumerator();
        }
    }

    public abstract class DictionaryFormatterBase<TKey, TValue, TDictionary> : DictionaryFormatterBase<TKey, TValue, TDictionary, TDictionary>
        where TDictionary : IDictionary<TKey, TValue>
    {
        protected override TDictionary Complete(TDictionary intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    public sealed class DictionaryFormatter<TKey, TValue> : DictionaryFormatterBase<TKey, TValue, Dictionary<TKey, TValue>, Dictionary<TKey, TValue>.Enumerator, Dictionary<TKey, TValue>>
    {
        protected override void Add(Dictionary<TKey, TValue> collection, int index, TKey key, TValue value, NetDataSerializerOptions options)
        {
            collection.Add(key, value);
        }

        protected override Dictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> intermediateCollection)
        {
            return intermediateCollection;
        }

        protected override Dictionary<TKey, TValue> Create(int count, NetDataSerializerOptions options)
        {
            return new Dictionary<TKey, TValue>(count);
        }

        protected override Dictionary<TKey, TValue>.Enumerator GetSourceEnumerator(Dictionary<TKey, TValue> source)
        {
            return source.GetEnumerator();
        }
    }

    public sealed class GenericDictionaryFormatter<TKey, TValue, TDictionary> : DictionaryFormatterBase<TKey, TValue, TDictionary>
        where TDictionary : IDictionary<TKey, TValue>, new()
    {
        protected override void Add(TDictionary collection, int index, TKey key, TValue value, NetDataSerializerOptions options)
        {
            collection.Add(key, value);
        }

        protected override TDictionary Create(int count, NetDataSerializerOptions options)
        {
            return new TDictionary();
        }
    }

    public sealed class InterfaceDictionaryFormatter<TKey, TValue> : DictionaryFormatterBase<TKey, TValue, Dictionary<TKey, TValue>, IDictionary<TKey, TValue>>
    {
        protected override void Add(Dictionary<TKey, TValue> collection, int index, TKey key, TValue value, NetDataSerializerOptions options)
        {
            collection.Add(key, value);
        }

        protected override Dictionary<TKey, TValue> Create(int count, NetDataSerializerOptions options)
        {
            return new Dictionary<TKey, TValue>(count);
        }

        protected override IDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    public sealed class SortedListFormatter<TKey, TValue> : DictionaryFormatterBase<TKey, TValue, SortedList<TKey, TValue>>
    {
        protected override void Add(SortedList<TKey, TValue> collection, int index, TKey key, TValue value, NetDataSerializerOptions options)
        {
            collection.Add(key, value);
        }

        protected override SortedList<TKey, TValue> Create(int count, NetDataSerializerOptions options)
        {
            return new SortedList<TKey, TValue>(count);
        }
    }

    public sealed class SortedDictionaryFormatter<TKey, TValue> : DictionaryFormatterBase<TKey, TValue, SortedDictionary<TKey, TValue>, SortedDictionary<TKey, TValue>.Enumerator, SortedDictionary<TKey, TValue>>
    {
        protected override void Add(SortedDictionary<TKey, TValue> collection, int index, TKey key, TValue value, NetDataSerializerOptions options)
        {
            collection.Add(key, value);
        }

        protected override SortedDictionary<TKey, TValue> Complete(SortedDictionary<TKey, TValue> intermediateCollection)
        {
            return intermediateCollection;
        }

        protected override SortedDictionary<TKey, TValue> Create(int count, NetDataSerializerOptions options)
        {
            return new SortedDictionary<TKey, TValue>();
        }

        protected override SortedDictionary<TKey, TValue>.Enumerator GetSourceEnumerator(SortedDictionary<TKey, TValue> source)
        {
            return source.GetEnumerator();
        }
    }

    public sealed class ReadOnlyDictionaryFormatter<TKey, TValue> : DictionaryFormatterBase<TKey, TValue, Dictionary<TKey, TValue>, ReadOnlyDictionary<TKey, TValue>>
    {
        protected override void Add(Dictionary<TKey, TValue> collection, int index, TKey key, TValue value, NetDataSerializerOptions options)
        {
            collection.Add(key, value);
        }

        protected override ReadOnlyDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> intermediateCollection)
        {
            return new ReadOnlyDictionary<TKey, TValue>(intermediateCollection);
        }

        protected override Dictionary<TKey, TValue> Create(int count, NetDataSerializerOptions options)
        {
            return new Dictionary<TKey, TValue>(count);
        }
    }

    public sealed class InterfaceReadOnlyDictionaryFormatter<TKey, TValue> : DictionaryFormatterBase<TKey, TValue, Dictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>>
    {
        protected override void Add(Dictionary<TKey, TValue> collection, int index, TKey key, TValue value, NetDataSerializerOptions options)
        {
            collection.Add(key, value);
        }

        protected override IReadOnlyDictionary<TKey, TValue> Complete(Dictionary<TKey, TValue> intermediateCollection)
        {
            return intermediateCollection;
        }

        protected override Dictionary<TKey, TValue> Create(int count, NetDataSerializerOptions options)
        {
            return new Dictionary<TKey, TValue>(count);
        }
    }

    public sealed class ConcurrentDictionaryFormatter<TKey, TValue> : DictionaryFormatterBase<TKey, TValue, System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>>
    {
        protected override void Add(ConcurrentDictionary<TKey, TValue> collection, int index, TKey key, TValue value, NetDataSerializerOptions options)
        {
            collection.TryAdd(key, value);
        }

        protected override ConcurrentDictionary<TKey, TValue> Create(int count, NetDataSerializerOptions options)
        {
            // concurrent dictionary can't access defaultConcurrecyLevel so does not use count overload.
            return new ConcurrentDictionary<TKey, TValue>();
        }
    }
}
