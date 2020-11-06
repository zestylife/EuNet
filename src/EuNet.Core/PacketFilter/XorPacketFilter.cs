using System;

namespace EuNet.Core
{
    public class XorPacketFilter : IPacketFilter
    {
        private byte[] _xorData;

        public IPacketFilter NextFilter => null;

        public XorPacketFilter(int seed = 36324016, int xorLength = 512)
        {
            _xorData = new byte[xorLength];

            Random rand = new Random(seed);
            rand.NextBytes(_xorData);
        }

        public int Encode(NetPacket packet)
        {
            int headerSize = packet.GetHeaderSize();
            int size = packet.Size;
            byte[] data = packet.RawData;

            for (int i = headerSize; i < size; ++i)
            {
                data[i] ^= _xorData[i % _xorData.Length];
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
                data[i] ^= _xorData[i % _xorData.Length];
            }

            return 0;
        }
    }
}
