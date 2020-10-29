namespace EuNet.Core
{
    public class NetDataSerializerOptions
    {
        public INetDataFormatterResolver Resolver { get; private set; }

        public static NetDataSerializerOptions Standard = new NetDataSerializerOptions(StandardResolver.Instance);

        public NetDataSerializerOptions(INetDataFormatterResolver resolver)
        {
            Resolver = resolver;
        }
    }
}
