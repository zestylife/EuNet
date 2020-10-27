namespace EuNet.Core
{
    public interface IChannelOption
    {
        IPacketFilter PacketFilter { get; set; }
        int PingInterval { get; set; }
        int MtuInterval { get; set; }
        int RudpDisconnectTimeout { get; set; }
    }
}
