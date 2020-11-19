using System.Net.Sockets;

namespace EuNet.Core
{
    /// <summary>
    /// 세션 초기화시 필요한 정보
    /// </summary>
    public ref struct SessionInitializeInfo
    {
        /// <summary>
        /// TCP에서 연결요청된 클라이언트 소켓 
        /// </summary>
        public Socket AcceptedTcpSocket;

        /// <summary>
        /// 서비스중인 UDP 소켓
        /// </summary>
        public UdpSocket UdpServiceSocket;
    }
}
