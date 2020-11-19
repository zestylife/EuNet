using System;

namespace EuNet.Core
{
    /// <summary>
    /// TEA 암호화와 Checksum 으로 검증하는 패킷필터
    /// </summary>
    public class TeaPacketFilter : IPacketFilter
    {
        private uint[] _keys;
        private const byte InitChecksumValue = 89;

        public IPacketFilter NextFilter => null;

        public TeaPacketFilter(int seed = 36324016)
        {
            _keys = CryptTea.GenerateKey(seed);
        }

        public NetPacket Encode(NetPacket packet)
        {
            // 필요한 사이즈를 계산하자
            int addSize = packet.Size - packet.GetHeaderSize() + 1;
            if (addSize % 8 != 0)
                addSize = 8 - (addSize % 8);

            // 패킷에 데이터가 남아있지 않으면 새로 할당하자
            packet = CheckAndResizePacket(packet, addSize);

            int headerSize = packet.GetHeaderSize();
            byte[] data = packet.RawData;
            int size = packet.Size;

            // 체크섬값 초기화
            data[size - 1] = InitChecksumValue;

            // 체크섬값을 마지막에 넣자
            data[size - 1] = GetChecksum(data, headerSize, size - headerSize);

            // tea
            CryptTea.EncryptSimple(data, headerSize, size - headerSize, _keys);

            return packet;
        }

        public NetPacket Decode(NetPacket packet)
        {
            int headerSize = packet.GetHeaderSize();
            int size = packet.Size;
            byte[] data = packet.RawData;

            // tea
            CryptTea.DecryptSimple(data, headerSize, size - headerSize, _keys);

            // checksum
            byte checksum = GetChecksum(data, headerSize, size - headerSize);

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
