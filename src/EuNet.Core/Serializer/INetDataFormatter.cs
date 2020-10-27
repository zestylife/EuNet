namespace EuNet.Core
{
    public interface INetDataFormatter
    {
    }

    public interface INetDataFormatter<T> : INetDataFormatter
    {
        void Serialize(NetDataWriter writer, T value, NetDataSerializerOptions options);
        T Deserialize(NetDataReader reader, NetDataSerializerOptions options);
    }
}
