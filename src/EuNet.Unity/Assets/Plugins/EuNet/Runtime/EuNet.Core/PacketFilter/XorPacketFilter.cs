using System;

namespace EuNet.Core
{
    // xor + checksum
    public class XorPacketFilter : IPacketFilter
    {
        private byte[] _xorData;
        private const byte InitChecksumValue = 89;

        public IPacketFilter NextFilter => null;

        public XorPacketFilter(int seed = 36324016, int xorLength = 512)
        {
            _xorData = new byte[xorLength];

            Random rand = new Random(seed);
            rand.NextBytes(_xorData);
        }

        public NetPacket Encode(NetPacket packet)
        {
            // 패킷에 데이터가 남아있지 않으면 새로 할당하자
            packet = CheckAndResizePacket(packet, 1);

            int headerSize = packet.GetHeaderSize();
            byte[] data = packet.RawData;
            int size = packet.Size;

            // 체크섬값 초기화
            data[size - 1] = InitChecksumValue;

            // 체크섬값을 마지막에 넣자
            data[size - 1] = GetBufferHash(data, 0, size);

            // xor
            for (int i = headerSize; i < size; ++i)
            {
                data[i] ^= _xorData[i % _xorData.Length];
            }

            return packet;
        }

        public NetPacket Decode(NetPacket packet)
        {
            int headerSize = packet.GetHeaderSize();
            int size = packet.Size;
            byte[] data = packet.RawData;

            for (int i = headerSize; i < size; ++i)
            {
                data[i] ^= _xorData[i % _xorData.Length];
            }

            byte checksum = GetBufferHash(data, 0, size);

            if (InitChecksumValue != checksum)
                throw new Exception("not match packet checksum");

            return packet;
        }

        private NetPacket CheckAndResizePacket(NetPacket poolingPacket, int addSize)
        {
            if (poolingPacket.RawData.Length < poolingPacket.Size + addSize)
            {
                // 버퍼가 작으므로 다시 할당하자
                var newPacket = NetPool.PacketPool.Alloc(poolingPacket, addSize);

                // 기존 패킷을 풀에 넣는다.
                NetPool.PacketPool.Free(poolingPacket);
                poolingPacket = newPacket;
            }
            else
            {
                poolingPacket.Size = (ushort)(poolingPacket.Size + addSize);
            }

            return poolingPacket;
        }

        private byte GetBufferHash(byte[] buffer, int offset, int length)
        {
            byte checksum = 0;
            for (int i = offset; i < length; ++i)
                checksum ^= buffer[i];

            return checksum;
        }
    }
}
