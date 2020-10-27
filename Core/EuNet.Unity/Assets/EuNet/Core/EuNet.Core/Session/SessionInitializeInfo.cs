using System.Net.Sockets;

namespace EuNet.Core
{
    public ref struct SessionInitializeInfo
    {
        // TCP에서 연결요청된 클라이언트 소켓
        public Socket AcceptedTcpSocket;

        // 서비스중인 UDP 소켓
        public UdpSocket UdpServiceSocket;
    }
}
