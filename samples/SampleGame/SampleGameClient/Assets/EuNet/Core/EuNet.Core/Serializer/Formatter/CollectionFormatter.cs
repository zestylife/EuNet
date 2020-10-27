// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#pragma warning disable SA1649 // File name should match first type name

namespace EuNet.Core
{
    public sealed class ArrayFormatter<T> : INetDataFormatter<T[]>
    {
        public void Serialize(NetDataWriter writer, T[] value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();

                writer.Write(value.Length);

                for (int i = 0; i < value.Length; i++)
                {
                    formatter.Serialize(writer, value[i], options);
                }
            }
        }

        public T[] Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }

            var len = reader.ReadInt32();
            if (len == 0)
            {
                return Array.Empty<T>();
            }

            INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();
            var array = new T[len];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = formatter.Deserialize(reader, options);
            }

            return array;
        }
    }

    public sealed class ByteArraySegmentFormatter : INetDataFormatter<ArraySegment<byte>>
    {
        public static readonly ByteArraySegmentFormatter Instance = new ByteArraySegmentFormatter();

        private ByteArraySegmentFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, ArraySegment<byte> value, NetDataSerializerOptions options)
        {
            if (value.Array == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(value.Array, value.Offset, value.Count);
            }
        }

        public ArraySegment<byte> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            return new ArraySegment<byte>(reader.ReadBytes().ToArray());
        }
    }

    public sealed class ArraySegmentFormatter<T> : INetDataFormatter<ArraySegment<T>>
    {
        public void Serialize(NetDataWriter writer, ArraySegment<T> value, NetDataSerializerOptions options)
        {
            if (value.Array == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                var formatter = options.Resolver.GetFormatter<T[]>();
                formatter.Serialize(writer, value.ToArray(), options);
            }
        }

        public ArraySegment<T> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                T[] array = options.Resolver.GetFormatter<T[]>().Deserialize(reader, options);
                return new ArraySegment<T>(array);
            }
        }
    }

    // List<T> is popular format, should avoid abstraction.
    public sealed class ListFormatter<T> : INetDataFormatter<List<T>>
    {
        public void Serialize(NetDataWriter writer, List<T> value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();

                var c = value.Count;
                writer.Write((int)c);

                for (int i = 0; i < c; i++)
                {
                    formatter.Serialize(writer, value[i], options);
                }
            }
        }

        public List<T> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default;
            }
            else
            {
                INetDataFormatter<T> formatter = options.Resolver.GetFormatter<T>();

                var len = reader.ReadInt32();
                var list = new List<T>((int)len);

                for (int i = 0; i < len; i++)
                {
                    list.Add(formatter.Deserialize(reader, options));
                }

                return list;
            }
        }
    }

    public abstract class CollectionFormatterBase<TElement, TIntermediate, TEnumerator, TCollection> : INetDataFormatter<TCollection>
        where TCollection : IEnumerable<TElement>
        where TEnumerator : IEnumerator<TElement>
    {
        public void Serialize(NetDataWriter writer, TCollection value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                INetDataFormatter<TElement> formatter = options.Resolver.GetFormatter<TElement>();

                // Optimize iteration(array is fastest)
                if (value is TElement[] array)
                {
                    writer.Write((int)array.Length);

                    foreach (TElement item in array)
                    {
                        formatter.Serialize(writer, item, options);
                    }
                }
                else
                {
                    // knows count or not.
                    var seqCount = this.GetCount(value);
                    writer.Write((int)seqCount.Value);

                    // Unity's foreach struct enumerator causes boxing so iterate manually.
                    using (var e = this.GetSourceEnumerator(value))
                    {
                        while (e.MoveNext())
                        {
                            formatter.Serialize(writer, e.Current, options);
                        }
                    }
                }
            }
        }

        public TCollection Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default(TCollection);
            }
            else
            {
                INetDataFormatter<TElement> formatter = options.Resolver.GetFormatter<TElement>();

                var len = reader.ReadInt32();

                TIntermediate list = this.Create(len, options);

                for (int i = 0; i < len; i++)
                {
                    this.Add(list, i, formatter.Deserialize(reader, options), options);
                }

                return this.Complete(list);
            }
        }

        // abstraction for serialize
        protected virtual int? GetCount(TCollection sequence)
        {
            var collection = sequence as ICollection<TElement>;
            if (collection != null)
            {
                return collection.Count;
            }
            else
            {
                var c2 = sequence as IReadOnlyCollection<TElement>;
                if (c2 != null)
                {
                    return c2.Count;
                }
            }

            return null;
        }

        // Some collections can use struct iterator, this is optimization path
        protected abstract TEnumerator GetSourceEnumerator(TCollection source);

        // abstraction for deserialize
        protected abstract TIntermediate Create(int count, NetDataSerializerOptions options);

        protected abstract void Add(TIntermediate collection, int index, TElement value, NetDataSerializerOptions options);

        protected abstract TCollection Complete(TIntermediate intermediateCollection);
    }

    public abstract class CollectionFormatterBase<TElement, TIntermediate, TCollection> : CollectionFormatterBase<TElement, TIntermediate, IEnumerator<TElement>, TCollection>
        where TCollection : IEnumerable<TElement>
    {
        protected override IEnumerator<TElement> GetSourceEnumerator(TCollection source)
        {
            return source.GetEnumerator();
        }
    }

    public abstract class CollectionFormatterBase<TElement, TCollection> : CollectionFormatterBase<TElement, TCollection, TCollection>
        where TCollection : IEnumerable<TElement>
    {
        protected sealed override TCollection Complete(TCollection intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    public sealed class GenericCollectionFormatter<TElement, TCollection> : CollectionFormatterBase<TElement, TCollection>
         where TCollection : ICollection<TElement>, new()
    {
        protected override TCollection Create(int count, NetDataSerializerOptions options)
        {
            return new TCollection();
        }

        protected override void Add(TCollection collection, int index, TElement value, NetDataSerializerOptions options)
        {
            collection.Add(value);
        }
    }

    public sealed class LinkedListFormatter<T> : CollectionFormatterBase<T, LinkedList<T>, LinkedList<T>.Enumerator, LinkedList<T>>
    {
        protected override void Add(LinkedList<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.AddLast(value);
        }

        protected override LinkedList<T> Complete(LinkedList<T> intermediateCollection)
        {
            return intermediateCollection;
        }

        protected override LinkedList<T> Create(int count, NetDataSerializerOptions options)
        {
            return new LinkedList<T>();
        }

        protected override LinkedList<T>.Enumerator GetSourceEnumerator(LinkedList<T> source)
        {
            return source.GetEnumerator();
        }
    }

    public sealed class QueueFormatter<T> : CollectionFormatterBase<T, Queue<T>, Queue<T>.Enumerator, Queue<T>>
    {
        protected override int? GetCount(Queue<T> sequence)
        {
            return sequence.Count;
        }

        protected override void Add(Queue<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.Enqueue(value);
        }

        protected override Queue<T> Create(int count, NetDataSerializerOptions options)
        {
            return new Queue<T>(count);
        }

        protected override Queue<T>.Enumerator GetSourceEnumerator(Queue<T> source)
        {
            return source.GetEnumerator();
        }

        protected override Queue<T> Complete(Queue<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    // should deserialize reverse order.
    public sealed class StackFormatter<T> : CollectionFormatterBase<T, T[], Stack<T>.Enumerator, Stack<T>>
    {
        protected override int? GetCount(Stack<T> sequence)
        {
            return sequence.Count;
        }

        protected override void Add(T[] collection, int index, T value, NetDataSerializerOptions options)
        {
            // add reverse
            collection[collection.Length - 1 - index] = value;
        }

        protected override T[] Create(int count, NetDataSerializerOptions options)
        {
            return count == 0 ? Array.Empty<T>() : new T[count];
        }

        protected override Stack<T>.Enumerator GetSourceEnumerator(Stack<T> source)
        {
            return source.GetEnumerator();
        }

        protected override Stack<T> Complete(T[] intermediateCollection)
        {
            return new Stack<T>(intermediateCollection);
        }
    }

    public sealed class HashSetFormatter<T> : CollectionFormatterBase<T, HashSet<T>, HashSet<T>.Enumerator, HashSet<T>>
    {
        protected override int? GetCount(HashSet<T> sequence)
        {
            return sequence.Count;
        }

        protected override void Add(HashSet<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override HashSet<T> Complete(HashSet<T> intermediateCollection)
        {
            return intermediateCollection;
        }

        protected override HashSet<T> Create(int count, NetDataSerializerOptions options)
        {
            return new HashSet<T>();
        }

        protected override HashSet<T>.Enumerator GetSourceEnumerator(HashSet<T> source)
        {
            return source.GetEnumerator();
        }
    }

    public sealed class ReadOnlyCollectionFormatter<T> : CollectionFormatterBase<T, T[], ReadOnlyCollection<T>>
    {
        protected override void Add(T[] collection, int index, T value, NetDataSerializerOptions options)
        {
            collection[index] = value;
        }

        protected override ReadOnlyCollection<T> Complete(T[] intermediateCollection)
        {
            return new ReadOnlyCollection<T>(intermediateCollection);
        }

        protected override T[] Create(int count, NetDataSerializerOptions options)
        {
            return count == 0 ? Array.Empty<T>() : new T[count];
        }
    }

    [Obsolete("Use " + nameof(InterfaceListFormatter2<int>) + " instead.")]
    public sealed class InterfaceListFormatter<T> : CollectionFormatterBase<T, T[], IList<T>>
    {
        protected override void Add(T[] collection, int index, T value, NetDataSerializerOptions options)
        {
            collection[index] = value;
        }

        protected override T[] Create(int count, NetDataSerializerOptions options)
        {
            return count == 0 ? Array.Empty<T>() : new T[count];
        }

        protected override IList<T> Complete(T[] intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    [Obsolete("Use " + nameof(InterfaceCollectionFormatter2<int>) + " instead.")]
    public sealed class InterfaceCollectionFormatter<T> : CollectionFormatterBase<T, T[], ICollection<T>>
    {
        protected override void Add(T[] collection, int index, T value, NetDataSerializerOptions options)
        {
            collection[index] = value;
        }

        protected override T[] Create(int count, NetDataSerializerOptions options)
        {
            return count == 0 ? Array.Empty<T>() : new T[count];
        }

        protected override ICollection<T> Complete(T[] intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    public sealed class InterfaceListFormatter2<T> : CollectionFormatterBase<T, List<T>, IList<T>>
    {
        protected override void Add(List<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override List<T> Create(int count, NetDataSerializerOptions options)
        {
            return new List<T>(count);
        }

        protected override IList<T> Complete(List<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    public sealed class InterfaceCollectionFormatter2<T> : CollectionFormatterBase<T, List<T>, ICollection<T>>
    {
        protected override void Add(List<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override List<T> Create(int count, NetDataSerializerOptions options)
        {
            return new List<T>(count);
        }

        protected override ICollection<T> Complete(List<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    public sealed class InterfaceEnumerableFormatter<T> : CollectionFormatterBase<T, T[], IEnumerable<T>>
    {
        protected override void Add(T[] collection, int index, T value, NetDataSerializerOptions options)
        {
            collection[index] = value;
        }

        protected override T[] Create(int count, NetDataSerializerOptions options)
        {
            return count == 0 ? Array.Empty<T>() : new T[count];
        }

        protected override IEnumerable<T> Complete(T[] intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    // [Key, [Array]]
    public sealed class InterfaceGroupingFormatter<TKey, TElement> : INetDataFormatter<IGrouping<TKey, TElement>>
    {
        public void Serialize(NetDataWriter writer, IGrouping<TKey, TElement> value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write((byte)2);
                options.Resolver.GetFormatter<TKey>().Serialize(writer, value.Key, options);
                options.Resolver.GetFormatter<IEnumerable<TElement>>().Serialize(writer, value, options);
            }
        }

        public IGrouping<TKey, TElement> Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return null;
            }
            else
            {
                var count = reader.ReadByte();

                if (count != 2)
                {
                    throw new NetDataSerializationException("Invalid Grouping format.");
                }

                TKey key = options.Resolver.GetFormatter<TKey>().Deserialize(reader, options);
                IEnumerable<TElement> value = options.Resolver.GetFormatter<IEnumerable<TElement>>().Deserialize(reader, options);
                return new Grouping<TKey, TElement>(key, value);
            }
        }
    }

    public sealed class InterfaceLookupFormatter<TKey, TElement> : CollectionFormatterBase<IGrouping<TKey, TElement>, Dictionary<TKey, IGrouping<TKey, TElement>>, ILookup<TKey, TElement>>
    {
        protected override void Add(Dictionary<TKey, IGrouping<TKey, TElement>> collection, int index, IGrouping<TKey, TElement> value, NetDataSerializerOptions options)
        {
            collection.Add(value.Key, value);
        }

        protected override ILookup<TKey, TElement> Complete(Dictionary<TKey, IGrouping<TKey, TElement>> intermediateCollection)
        {
            return new Lookup<TKey, TElement>(intermediateCollection);
        }

        protected override Dictionary<TKey, IGrouping<TKey, TElement>> Create(int count, NetDataSerializerOptions options)
        {
            return new Dictionary<TKey, IGrouping<TKey, TElement>>(count);
        }
    }

    internal class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly TKey key;
        private readonly IEnumerable<TElement> elements;

        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            this.key = key;
            this.elements = elements;
        }

        public TKey Key
        {
            get
            {
                return this.key;
            }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }
    }

    internal class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        private readonly Dictionary<TKey, IGrouping<TKey, TElement>> groupings;

        public Lookup(Dictionary<TKey, IGrouping<TKey, TElement>> groupings)
        {
            this.groupings = groupings;
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                return this.groupings[key];
            }
        }

        public int Count
        {
            get
            {
                return this.groupings.Count;
            }
        }

        public bool Contains(TKey key)
        {
            return this.groupings.ContainsKey(key);
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return this.groupings.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.groupings.Values.GetEnumerator();
        }
    }

    /* NonGenerics */

    public sealed class NonGenericListFormatter<T> : INetDataFormatter<T>
        where T : class, IList, new()
    {
        public void Serialize(NetDataWriter writer, T value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);
            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            writer.Write((int)value.Count);
            foreach (var item in value)
            {
                formatter.Serialize(writer, item, options);
            }
        }

        public T Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default(T);
            }

            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            var count = reader.ReadInt32();

            var list = new T();

            for (int i = 0; i < count; i++)
            {
                list.Add(formatter.Deserialize(reader, options));
            }

            return list;
        }
    }

    public sealed class NonGenericInterfaceCollectionFormatter : INetDataFormatter<ICollection>
    {
        public static readonly INetDataFormatter<ICollection> Instance = new NonGenericInterfaceCollectionFormatter();

        private NonGenericInterfaceCollectionFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, ICollection value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);
            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            writer.Write((int)value.Count);
            foreach (var item in value)
            {
                formatter.Serialize(writer, item, options);
            }
        }

        public ICollection Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default(ICollection);
            }

            var count = reader.ReadInt32();
            if (count == 0)
            {
                return Array.Empty<object>();
            }

            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            var list = new object[count];

            for (int i = 0; i < count; i++)
            {
                list[i] = formatter.Deserialize(reader, options);
            }

            return list;
        }
    }

    public sealed class NonGenericInterfaceEnumerableFormatter : INetDataFormatter<IEnumerable>
    {
        public static readonly INetDataFormatter<IEnumerable> Instance = new NonGenericInterfaceEnumerableFormatter();

        private NonGenericInterfaceEnumerableFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, IEnumerable value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);
            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            int prevPos = writer.Length;
            writer.Write((int)0);
            int count = 0;
            foreach (var item in value)
            {
                formatter.Serialize(writer, item, options);
                count++;
            }

            int resultPos = writer.Length;
            writer.Length = prevPos;
            writer.Write(count);
            writer.Length = resultPos;
        }

        public IEnumerable Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default(IEnumerable);
            }

            var count = reader.ReadInt32();
            if (count == 0)
            {
                return Array.Empty<object>();
            }

            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            var list = new object[count];

            for (int i = 0; i < count; i++)
            {
                list[i] = formatter.Deserialize(reader, options);
            }

            return list;
        }
    }

    public sealed class NonGenericInterfaceListFormatter : INetDataFormatter<IList>
    {
        public static readonly INetDataFormatter<IList> Instance = new NonGenericInterfaceListFormatter();

        private NonGenericInterfaceListFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, IList value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);
            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            writer.Write((int)value.Count);
            foreach (var item in value)
            {
                formatter.Serialize(writer, item, options);
            }
        }

        public IList Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return default(IList);
            }

            var count = reader.ReadInt32();
            if (count == 0)
            {
                return Array.Empty<object>();
            }

            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            var list = new object[count];

            for (int i = 0; i < count; i++)
            {
                list[i] = formatter.Deserialize(reader, options);
            }

            return list;
        }
    }

    public sealed class NonGenericDictionaryFormatter<T> : INetDataFormatter<T>
        where T : class, IDictionary, new()
    {
        public void Serialize(NetDataWriter writer, T value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);
            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            writer.Write((int)value.Count);
            foreach (DictionaryEntry item in value)
            {
                formatter.Serialize(writer, item.Key, options);
                formatter.Serialize(writer, item.Value, options);
            }
        }

        public T Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return null;
            }

            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            var count = reader.ReadInt32();

            var dict = new T();

            for (int i = 0; i < count; i++)
            {
                var key = formatter.Deserialize(reader, options);
                var value = formatter.Deserialize(reader, options);
                dict.Add(key, value);
            }

            return dict;
        }
    }

    public sealed class NonGenericInterfaceDictionaryFormatter : INetDataFormatter<IDictionary>
    {
        public static readonly INetDataFormatter<IDictionary> Instance = new NonGenericInterfaceDictionaryFormatter();

        private NonGenericInterfaceDictionaryFormatter()
        {
        }

        public void Serialize(NetDataWriter writer, IDictionary value, NetDataSerializerOptions options)
        {
            if (value == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);
            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            writer.Write((int)value.Count);
            foreach (DictionaryEntry item in value)
            {
                formatter.Serialize(writer, item.Key, options);
                formatter.Serialize(writer, item.Value, options);
            }
        }

        public IDictionary Deserialize(NetDataReader reader, NetDataSerializerOptions options)
        {
            if (reader.ReadBoolean() == false)
            {
                return null;
            }

            INetDataFormatter<object> formatter = options.Resolver.GetFormatter<object>();

            var count = reader.ReadInt32();

            var dict = new Dictionary<object, object>(count);

            for (int i = 0; i < count; i++)
            {
                var key = formatter.Deserialize(reader, options);
                var value = formatter.Deserialize(reader, options);
                dict.Add(key, value);
            }

            return dict;
        }
    }

    public sealed class ObservableCollectionFormatter<T> : CollectionFormatterBase<T, ObservableCollection<T>>
    {
        protected override void Add(ObservableCollection<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override ObservableCollection<T> Create(int count, NetDataSerializerOptions options)
        {
            return new ObservableCollection<T>();
        }
    }

    public sealed class ReadOnlyObservableCollectionFormatter<T> : CollectionFormatterBase<T, ObservableCollection<T>, ReadOnlyObservableCollection<T>>
    {
        protected override void Add(ObservableCollection<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override ObservableCollection<T> Create(int count, NetDataSerializerOptions options)
        {
            return new ObservableCollection<T>();
        }

        protected override ReadOnlyObservableCollection<T> Complete(ObservableCollection<T> intermediateCollection)
        {
            return new ReadOnlyObservableCollection<T>(intermediateCollection);
        }
    }

    public sealed class InterfaceReadOnlyListFormatter<T> : CollectionFormatterBase<T, T[], IReadOnlyList<T>>
    {
        protected override void Add(T[] collection, int index, T value, NetDataSerializerOptions options)
        {
            collection[index] = value;
        }

        protected override T[] Create(int count, NetDataSerializerOptions options)
        {
            return count == 0 ? Array.Empty<T>() : new T[count];
        }

        protected override IReadOnlyList<T> Complete(T[] intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    public sealed class InterfaceReadOnlyCollectionFormatter<T> : CollectionFormatterBase<T, T[], IReadOnlyCollection<T>>
    {
        protected override void Add(T[] collection, int index, T value, NetDataSerializerOptions options)
        {
            collection[index] = value;
        }

        protected override T[] Create(int count, NetDataSerializerOptions options)
        {
            return count == 0 ? Array.Empty<T>() : new T[count];
        }

        protected override IReadOnlyCollection<T> Complete(T[] intermediateCollection)
        {
            return intermediateCollection;
        }
    }

    public sealed class InterfaceSetFormatter<T> : CollectionFormatterBase<T, HashSet<T>, ISet<T>>
    {
        protected override void Add(HashSet<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override ISet<T> Complete(HashSet<T> intermediateCollection)
        {
            return intermediateCollection;
        }

        protected override HashSet<T> Create(int count, NetDataSerializerOptions options)
        {
            return new HashSet<T>();
        }
    }

    public sealed class ConcurrentBagFormatter<T> : CollectionFormatterBase<T, System.Collections.Concurrent.ConcurrentBag<T>>
    {
        protected override int? GetCount(ConcurrentBag<T> sequence)
        {
            return sequence.Count;
        }

        protected override void Add(ConcurrentBag<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override ConcurrentBag<T> Create(int count, NetDataSerializerOptions options)
        {
            return new ConcurrentBag<T>();
        }
    }

    public sealed class ConcurrentQueueFormatter<T> : CollectionFormatterBase<T, System.Collections.Concurrent.ConcurrentQueue<T>>
    {
        protected override int? GetCount(ConcurrentQueue<T> sequence)
        {
            return sequence.Count;
        }

        protected override void Add(ConcurrentQueue<T> collection, int index, T value, NetDataSerializerOptions options)
        {
            collection.Enqueue(value);
        }

        protected override ConcurrentQueue<T> Create(int count, NetDataSerializerOptions options)
        {
            return new ConcurrentQueue<T>();
        }
    }

    public sealed class ConcurrentStackFormatter<T> : CollectionFormatterBase<T, T[], ConcurrentStack<T>>
    {
        protected override int? GetCount(ConcurrentStack<T> sequence)
        {
            return sequence.Count;
        }

        protected override void Add(T[] collection, int index, T value, NetDataSerializerOptions options)
        {
            // add reverse
            collection[collection.Length - 1 - index] = value;
        }

        protected override T[] Create(int count, NetDataSerializerOptions options)
        {
            return count == 0 ? Array.Empty<T>() : new T[count];
        }

        protected override ConcurrentStack<T> Complete(T[] intermediateCollection)
        {
            return new ConcurrentStack<T>(intermediateCollection);
        }
    }
}
