namespace EuNet.Core
{
    public class SessionCreateInfo
    {
        public ushort SessionId;
        public TcpChannel TcpChannel;
        public UdpChannel UdpChannel;
        public NetStatistic Statistic;
    }
}
