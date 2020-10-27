
namespace EuNet.Core
{
    // 패킷을 가공하는 필터 클래스
    public interface IPacketFilter
    {
        IPacketFilter NextFilter { get; }

        // 보내기 전에 데이터를 가공한다 (암호화, 체크섬 생성 등)
        int Encode(NetPacket packet);

        // 받은 후 데이터를 사용할 수 있게 가공한다 (복호화, 체크섬 검증 등). 내부 버퍼를 재할당하면 안됨
        int Decode(NetPacket packet);
    }
}
