namespace EuNet.Core
{
    public class UnreliableChannel : ChannelBase
    {
        private UdpChannel _udpChannel;

        public UnreliableChannel(IChannelOption channelOption, ILogger logger, NetStatistic statistic, UdpChannel channel)
            : base(channelOption, logger, statistic)
        {
            _udpChannel = channel;
        }

        public override bool Update(int elapsedTime)
        {
            return true;
        }

        public override void SendAsync(NetPacket poolingPacket)
        {
            try
            {
                _udpChannel.SendTo(poolingPacket.RawData, 0, poolingPacket.Size, UdpChannel.SendMode.Buffered);
            }
            finally
            {
                NetPool.PacketPool.Free(poolingPacket);
            }
        }
    }
}
