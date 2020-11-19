namespace EuNet.Core
{
    /// <summary>
    /// 네트워크 전송을 위한 직렬화 인터페이스
    /// 이 인터페이스를 상속받아서 구현하면 빠른 직렬화 가능
    /// </summary>
    public interface INetSerializable
    {
        void Serialize(NetDataWriter writer);
        void Deserialize(NetDataReader reader);
    }
}
