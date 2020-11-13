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

        // Tcp KeepAlive 사용여부
        public bool TcpKeepAlive { get; set; } = false;

        // Tcp KeepAlive 를 보내서 체크하는 주기 (초)
        public int TcpKeepAliveTime { get; set; } = 60;

        // Tcp KeepAliveTime이 지나고 처리되지 않았을때 재시도 간격 (초)
        public int TcpKeepAliveInterval { get; set; } = 1;

        // Tcp TcpKeepAliveInterval 별로 재시도 회수
        public int TcpKeepAliveRetryCount { get; set; } = 10;

        public bool IsServiceUdp { get; set; } = false;
        public string UdpServerAddress { get; set; } = "any";
        public int UdpServerPort { get; set; }
        public bool UdpReuseAddress { get; set; } = true;

        public IPacketFilter PacketFilter { get; set; }
        public int PingInterval { get; set; } = 1000;
        public int MtuInterval { get; set; } = 1100;
        public int RudpDisconnectTimeout { get; set; } = 5000;

        // 살아있는지 체크여부
        public bool IsCheckAlive { get; set; } = false;

        // 이 주기로 한번씩 패킷을 전송함 (ms)
        public long CheckAliveInterval { get; set; } = 40000;

        // 이 시간동안 서버로 패킷이 오지 않으면 접속해제 (ms)
        public long CheckAliveTimeout { get; set; } = 60000;
    }
}
