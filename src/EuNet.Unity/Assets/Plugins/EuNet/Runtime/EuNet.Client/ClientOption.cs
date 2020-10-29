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
    }
}
