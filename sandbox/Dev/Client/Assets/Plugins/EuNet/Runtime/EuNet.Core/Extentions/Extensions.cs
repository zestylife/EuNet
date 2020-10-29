using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EuNet.Core
{
    public static class Extensions
    {

#if NETSTANDARD2_0 || UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS

        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item))
            {

            }
        }
#endif

        public static void RemoveAllByValue<K, V>(this Dictionary<K, V> dictionary, V value)
        {
            foreach (var key in dictionary.Where(kvp => kvp.Value.Equals(value)).Select(x => x.Key).ToArray())
                dictionary.Remove(key);
        }

        public static void RemoveAllByValue<K, V>(this ConcurrentDictionary<K, V> dictionary, V value)
        {
            V delValue;
            foreach (var key in dictionary.Where(kvp => kvp.Value.Equals(value)).Select(x => x.Key).ToArray())
                dictionary.TryRemove(key, out delValue);
        }
    }
}
