using EuNet.Core;

namespace EuNet.Client
{
    public class ClientOption : IChannelOption
    {
        public string TcpServerAddress { get; set; }
        public int TcpServerPort { get; set; }

        public bool IsServiceUdp { get; set; } = false;
        public string UdpServerAddress { get; set; }
        public int UdpServerPort { get; set; }

        public IPacketFilter PacketFilter { get; set; }
        public int PingInterval { get; set; } = 1000;
        public int MtuInterval { get; set; } = 1100;
        public int RudpDisconnectTimeout { get; set; } = 5000;

        // 강제로 릴레이시킴 (홀펀칭 시도 안함. 테스트 전용)
        public bool IsForceRelay { get; set; } = false;

        // 살아있는지 체크여부
        public bool IsCheckAlive { get; set; } = false;

        // 이 주기로 한번씩 패킷을 전송함 (ms)
        public long CheckAliveInterval { get; set; } = 40000;

        // 이 시간동안 서버로 패킷이 오지 않으면 접속해제 (ms)
        public long CheckAliveTimeout { get; set; } = 60000;
    }
}
