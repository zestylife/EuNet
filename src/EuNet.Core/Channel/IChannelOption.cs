namespace EuNet.Core
{
    /// <summary>
    /// 채널 옵션 인터페이스
    /// </summary>
    public interface IChannelOption
    {
        /// <summary>
        /// 패킷 필터
        /// </summary>
        IPacketFilter PacketFilter { get; set; }

        /// <summary>
        /// 주기적으로 Ping 을 보낼 간격 (ms)
        /// </summary>
        int PingInterval { get; set; }

        /// <summary>
        /// 주기적으로 Mtu 를 보낼 간격 (ms). Mtu 체크가 끝나면 보내지 않는다
        /// </summary>
        int MtuInterval { get; set; }

        /// <summary>
        /// Reliable Udp 패킷을 보내고 RudpDisconnectTimeout 시간동안 Ack 패킷을 받지 못하면 접속이 해제된다 (ms)
        /// </summary>
        int RudpDisconnectTimeout { get; set; }

        /// <summary>
        /// 내장된 CheckAlive 기능의 사용 여부 (TCP Only)
        /// </summary>
        bool IsCheckAlive { get; set; }

        /// <summary>
        /// 주기적으로 CheckAlive 패킷을 보낼 간격 (ms)
        /// </summary>
        long CheckAliveInterval { get; set; }

        /// <summary>
        /// 이 시간 동안 CheckAlive를 포함한 어떠한 패킷도 오지 않으면 접속이 해제된다 (ms)
        /// </summary>
        long CheckAliveTimeout { get; set; }
    }
}
