namespace EuNet.Core
{
    /// <summary>
    /// 세션 생성시 필요한 정보
    /// </summary>
    public class SessionCreateInfo
    {
        public ushort SessionId;
        public TcpChannel TcpChannel;
        public UdpChannel UdpChannel;
        public NetStatistic Statistic;
    }
}
