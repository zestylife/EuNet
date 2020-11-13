namespace EuNet.Core
{
    public interface IChannelOption
    {
        IPacketFilter PacketFilter { get; set; }
        int PingInterval { get; set; }
        int MtuInterval { get; set; }
        int RudpDisconnectTimeout { get; set; }

        // 살아있는지 체크여부
        bool IsCheckAlive { get; set; }

        // 이 주기로 한번씩 패킷을 전송함
        long CheckAliveInterval { get; set; }

        // 이 시간동안 서버로 패킷이 오지 않으면 접속해제
        long CheckAliveTimeout { get; set; }
    }
}
