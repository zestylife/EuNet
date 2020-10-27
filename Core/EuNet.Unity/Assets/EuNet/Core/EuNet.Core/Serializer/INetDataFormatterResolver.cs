namespace EuNet.Core
{
    public interface INetDataFormatterResolver
    {
        INetDataFormatter<T> GetFormatter<T>();
    }
}
