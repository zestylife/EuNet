
namespace EuNet.Core
{
    /// <summary>
    /// 패킷을 가공하는 필터 클래스.
    /// 암호화, 검증, 압축 등의 작업을 할 수 있음.
    /// </summary>
    public interface IPacketFilter
    {
        /// <summary>
        /// 필터 처리 후 사용될 다음 필터
        /// </summary>
        IPacketFilter NextFilter { get; }

        /// <summary>
        /// 보내기 전에 데이터를 가공한다.
        /// 풀링패킷을 받으며, 반드시 풀링패킷을 리턴해야함.
        /// 암호화, 검증, 압축 등의 작업을 할 수 있음.
        /// </summary>
        /// <param name="packet">수정되지 않은 원본 풀링패킷</param>
        /// <returns>Encode된 풀링패킷</returns>
        NetPacket Encode(NetPacket packet);

        /// <summary>
        /// 받은 후 데이터를 사용할 수 있게 가공한다.
        /// 풀링패킷을 받으며, 반드시 풀링패킷을 리턴해야함.
        /// 암호화, 검증, 압축 등의 작업을 할 수 있음.
        /// </summary>
        /// <param name="packet">Encode된 풀링패킷</param>
        /// <returns>원본상태로 복구된 풀링패킷</returns>
        NetPacket Decode(NetPacket packet);
    }
}
