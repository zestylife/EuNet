namespace EuNet.Core
{
    /// <summary>
    /// 패킷 특성
    /// 0x0F 영역만 사용하므로 (0~15)까지만 사용가능
    /// </summary>
    public enum PacketProperty : byte
    {
        /// <summary>
        /// 유저 데이터 패킷
        /// </summary>
        UserData,

        /// <summary>
        /// 요청
        /// </summary>
        Request,

        /// <summary>
        /// P2P의 NetView 요청
        /// </summary>
        ViewRequest,

        /// <summary>
        /// Alive Check
        /// </summary>
        AliveCheck,

        /// <summary>
        /// P2P 그룹에 가입
        /// </summary>
        JoinP2p,

        /// <summary>
        /// P2P 그룹에서 떠남
        /// </summary>
        LeaveP2p,

        /// <summary>
        /// P2P 마스터가 변경됨
        /// </summary>
        ChangeP2pMaster,

        /// <summary>
        /// P2P 홀펀칭 시작패킷 (홀펀칭 시도패킷)
        /// </summary>
        HolePunchingStart,

        /// <summary>
        /// P2P 홀펀칭 마무리패킷 (이 패킷을 받으면 홀펀칭 완료)
        /// </summary>
        HolePunchingEnd,

        /// <summary>
        /// RUDP Ack
        /// UDP Buffered Send를 이용하지 않고 즉시 보낸다.
        /// </summary>
        Ack,

        /// <summary>
        /// Ping
        /// UDP Buffered Send를 이용하지 않고 즉시 보낸다.
        /// </summary>
        Ping,

        /// <summary>
        /// Ping 에 대한 응답
        /// UDP Buffered Send를 이용하지 않고 즉시 보낸다.
        /// </summary>
        Pong,

        /// <summary>
        /// 연결 요청
        /// UDP Buffered Send를 이용하지 않고 즉시 보낸다.
        /// </summary>
        RequestConnection,

        /// <summary>
        /// 연결 응답
        /// UDP Buffered Send를 이용하지 않고 즉시 보낸다.
        /// </summary>
        ResponseConnection,

        /// <summary>
        /// Mtu Check
        /// UDP Buffered Send를 이용하지 않고 즉시 보낸다.
        /// </summary>
        MtuCheck,

        /// <summary>
        /// Mtu Ok
        /// UDP Buffered Send를 이용하지 않고 즉시 보낸다.
        /// </summary>
        MtuOk,
    }
}
