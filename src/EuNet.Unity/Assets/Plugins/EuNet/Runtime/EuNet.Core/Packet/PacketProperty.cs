namespace EuNet.Core
{
    // 패킷해더 특성상 0x0F (0~15)까지만 사용가능
    public enum PacketProperty : byte
    {
        // 유저 데이터 패킷
        UserData,

        // 내부 패킷
        Request,
        ViewRequest,
        AliveCheck,

        // P2p 관련
        JoinP2p,
        LeaveP2p,
        ChangeP2pMaster,
        HolePunchingStart,
        HolePunchingEnd,

        // 아래의 프로퍼티는 버퍼를 이용하지 않고 직접 보낸다
        Ack,
        Ping,
        Pong,
        RequestConnection,
        ResponseConnection,
        MtuCheck,
        MtuOk,
    }
}
