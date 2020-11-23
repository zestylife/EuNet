using System;

namespace EuNet.Core
{
    /// <summary>
    /// XOR 암호화와 Checksum 으로 검증하는 패킷필터
    /// </summary>
    public class XorPacketFilter : IPacketFilter
    {
        private byte[] _xorKey;
        private const byte InitChecksumValue = 89;

        public IPacketFilter NextFilter => null;

        public XorPacketFilter(int seed = 36324016, int keyLength = 1024)
        {
            _xorKey = CryptXor.GenerateKey(seed, keyLength);
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
            data[size - 1] = GetChecksum(data, headerSize, size - headerSize);

            // xor
            CryptXor.Crypt(data, headerSize, size - headerSize, _xorKey);
            
            return packet;
        }

        public NetPacket Decode(NetPacket packet)
        {
            int headerSize = packet.GetHeaderSize();
            int size = packet.Size;
            byte[] data = packet.RawData;

            // xor
            CryptXor.Crypt(data, headerSize, size - headerSize, _xorKey);

            // checksum
            byte checksum = GetChecksum(data, headerSize, size - headerSize);

            if (InitChecksumValue != checksum)
                throw new Exception($"Not match packet checksum [{packet.Property}]");

            packet.Size -= 1;

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
                poolingPacket.Size += (ushort)addSize;
            }

            return poolingPacket;
        }

        private byte GetChecksum(byte[] buffer, int offset, int length)
        {
            byte checksum = 0;
            for (int i = offset; i < offset + length; ++i)
                checksum ^= buffer[i];

            return checksum;
        }
    }
}
