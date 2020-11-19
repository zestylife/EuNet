using EuNet.Core;

namespace EuNet.Client
{
    /// <summary>
    /// 클라이언트 옵션
    /// </summary>
    public class ClientOption : IChannelOption
    {
        /// <summary>
        /// 접속할 서버 주소
        /// </summary>
        public string TcpServerAddress { get; set; }

        /// <summary>
        /// 접속할 서버 포트
        /// </summary>
        public int TcpServerPort { get; set; }

        /// <summary>
        /// UDP 및 P2P 사용여부 (서버도 동일하게 셋팅되어야 한다)
        /// </summary>
        public bool IsServiceUdp { get; set; } = false;
        public string UdpServerAddress { get; set; }
        public int UdpServerPort { get; set; }

        public IPacketFilter PacketFilter { get; set; }
        public int PingInterval { get; set; } = 1000;
        public int MtuInterval { get; set; } = 1100;
        public int RudpDisconnectTimeout { get; set; } = 5000;

        /// <summary>
        /// 모든 UDP 패킷을 강제로 릴레이시킴 (홀펀칭 시도 안함. 테스트 전용)
        /// </summary>
        public bool IsForceRelay { get; set; } = false;

        public bool IsCheckAlive { get; set; } = false;
        public long CheckAliveInterval { get; set; } = 30000;
        public long CheckAliveTimeout { get; set; } = 50000;
    }
}
