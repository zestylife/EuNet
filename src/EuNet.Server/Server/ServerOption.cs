using EuNet.Core;

namespace EuNet.Server
{
    /// <summary>
    /// 서버 옵션
    /// </summary>
    public class ServerOption : IChannelOption
    {
        /// <summary>
        /// 서버 이름
        /// </summary>
        public string Name { get; set; } = "server";

        /// <summary>
        /// 서버의 최대 허용 세션수. SessionFactory에서 세션풀링을 사용한다면 미리 할당된 갯수
        /// </summary>
        public int MaxSession { get; set; } = 10000;

        /// <summary>
        /// 세션의 업데이트 주기 (ms)
        /// CheckAlive, RUDP, Buffered UDP, Relay 등의 처리 주기
        /// 주기가 짧으면 Buffered Relay 데이터의 전송을 빠르게 할 수 있지만 CPU사용량이 늘어남
        /// </summary>
        public int SessionUpdateInternval { get; set; } = 30;

        public string TcpServerAddress { get; set; } = "any";
        public int TcpServerPort { get; set; }
        public int TcpBackLog { get; set; } = 512;
        public bool TcpNoDelay { get; set; } = false;

        /// <summary>
        /// Tcp KeepAlive 사용여부 (Socket Option. OS에 따라 작동이 안될 수 있음)
        /// </summary>
        public bool TcpKeepAlive { get; set; } = false;

        /// <summary>
        /// Tcp KeepAlive 를 보내서 체크하는 주기 (초) (Socket Option. OS에 따라 작동이 안될 수 있음)
        /// </summary>
        public int TcpKeepAliveTime { get; set; } = 60;

        /// <summary>
        /// Tcp KeepAliveTime이 지나고 처리되지 않았을때 재시도 간격 (초) (Socket Option. OS에 따라 작동이 안될 수 있음)
        /// </summary>
        public int TcpKeepAliveInterval { get; set; } = 1;

        /// <summary>
        /// Tcp TcpKeepAliveInterval 별로 재시도 회수 (Socket Option. OS에 따라 작동이 안될 수 있음)
        /// </summary>
        public int TcpKeepAliveRetryCount { get; set; } = 10;

        /// <summary>
        /// UDP 및 P2P 사용 여부
        /// </summary>
        public bool IsServiceUdp { get; set; } = false;
        public string UdpServerAddress { get; set; } = "any";
        public int UdpServerPort { get; set; }
        public bool UdpReuseAddress { get; set; } = true;

        /// <summary>
        /// 패킷 필터
        /// </summary>
        public IPacketFilter PacketFilter { get; set; }

        /// <summary>
        /// Ping 주기 (ms)
        /// </summary>
        public int PingInterval { get; set; } = 1000;

        /// <summary>
        /// Mtu Check 주기 (ms)
        /// </summary>
        public int MtuInterval { get; set; } = 1100;

        /// <summary>
        /// RUDP 타임아웃 (ms)
        /// </summary>
        public int RudpDisconnectTimeout { get; set; } = 5000;

        /// <summary>
        /// 내장된 CheckAlive 기능의 사용 여부 (TCP Only)
        /// </summary>
        public bool IsCheckAlive { get; set; } = false;

        /// <summary>
        /// 주기적으로 CheckAlive 패킷을 보낼 간격 (ms)
        /// </summary>
        public long CheckAliveInterval { get; set; } = 40000;

        /// <summary>
        /// 이 시간 동안 CheckAlive를 포함한 어떠한 패킷도 오지 않으면 접속이 해제된다 (ms)
        /// </summary>
        public long CheckAliveTimeout { get; set; } = 60000;
    }
}
