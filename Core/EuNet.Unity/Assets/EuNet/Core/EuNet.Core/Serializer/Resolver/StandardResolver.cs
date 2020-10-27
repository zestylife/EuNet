namespace EuNet.Core
{
    public sealed class StandardResolver : INetDataFormatterResolver
    {
        public static readonly StandardResolver Instance;
        public static readonly NetDataSerializerOptions Options;

        private static readonly INetDataFormatterResolver[] Resolvers =
        {
            BasicResolver.Instance,
            CustomResolver.Instance,
            DynamicGenericResolver.Instance,
        };

        static StandardResolver()
        {
            Instance = new StandardResolver();
            Options = new NetDataSerializerOptions(Instance);
        }

        private StandardResolver()
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
