using EuNet.Core;

namespace EuNet.Server
{
    public class ServerOption : IChannelOption
    {
        // 서버 이름
        public string Name { get; set; } = "server";

        // 서버 최대 세션수
        public int MaxSession { get; set; } = 10000;

        // 세션 업데이트 인터벌
        public int SessionUpdateInternval { get; set; } = 30;

        public string TcpServerAddress { get; set; } = "any";
        public int TcpServerPort { get; set; }
        public int TcpBackLog { get; set; } = 512;
        public bool TcpNoDelay { get; set; } = false;

        public bool IsServiceUdp { get; set; } = false;
        public string UdpServerAddress { get; set; } = "any";
        public int UdpServerPort { get; set; }
        public bool UdpReuseAddress { get; set; } = true;

        public IPacketFilter PacketFilter { get; set; }
        public int PingInterval { get; set; } = 1000;
        public int MtuInterval { get; set; } = 1100;
        public int RudpDisconnectTimeout { get; set; } = 5000;
    }
}
