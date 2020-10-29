using System.Collections.Generic;

namespace EuNet.Core
{
    public sealed class CustomResolver : INetDataFormatterResolver
    {
        public static readonly CustomResolver Instance;
        private static List<INetDataFormatterResolver> Resolvers = new List<INetDataFormatterResolver>();

        static CustomResolver()
        {
            Instance = new CustomResolver();
        }

        private CustomResolver()
        {
        }

        public static void Register(params INetDataFormatterResolver[] resolvers)
        {
            foreach(var resolver in resolvers)
            {
                if (Resolvers.Contains(resolver))
                    continue;

                Resolvers.Add(resolver);
            }
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
                foreach (INetDataFormatterResolver item in Resolvers)
                {
                    INetDataFormatter<T> f = item.GetFormatter<T>();
                    if (f != null)
                    {
                        Formatter = f;
                        return;
                    }
                }
            }
        }
    }
}
