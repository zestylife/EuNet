namespace EuNet.Core
{
    public class XorPacketFilter : IPacketFilter
    {
        public IPacketFilter NextFilter => null;

        public int Encode(NetPacket packet)
        {
            int headerSize = packet.GetHeaderSize();
            int size = packet.Size;
            byte[] data = packet.RawData;

            for (int i = headerSize; i < size; ++i)
            {
                data[i] ^= 0xAA;
            }

            return 0;
        }

        public int Decode(NetPacket packet)
        {
            int headerSize = packet.GetHeaderSize();
            int size = packet.Size;
            byte[] data = packet.RawData;

            for (int i = headerSize; i < size; ++i)
            {
                data[i] ^= 0xAA;
            }

            return 0;
        }
    }
}
