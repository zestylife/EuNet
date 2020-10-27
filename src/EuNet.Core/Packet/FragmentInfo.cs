using System;

namespace EuNet.Core
{
    internal class FragmentInfo
    {
        public NetPacket[] Fragments;

        // 파편화된 전체 갯수
        public int TotalCount => Fragments.Length;

        // 받은 파편 갯수
        public int ReceivedCount;

        // 받은 사이즈 (헤더를 제외한 데이터만)
        public int ReceivedSize;

        public DeliveryMethod DeliveryMethod;

        public FragmentInfo(NetPacket packet)
        {
            Fragments = new NetPacket[packet.FragmentsTotal];
            DeliveryMethod = packet.DeliveryMethod;
        }

        // 부서진 패킷을 넣어 새로운 패킷이 만들어지면 받는다
        public NetPacket AddFragment(NetPacket packet)
        {
            if (packet.IsFragmented == false)
                return null;

            if (packet.FragmentPart >= Fragments.Length ||
                Fragments[packet.FragmentPart] != null ||
                packet.DeliveryMethod != DeliveryMethod)
            {
                return null;
            }

            Fragments[packet.FragmentPart] = packet;
            ReceivedCount++;
            ReceivedSize += packet.Size - NetPacket.FragmentedHeaderTotalSize;

            if (ReceivedCount != Fragments.Length)
                return null;

            NetPacket resultingPacket = NetPool.PacketPool.Alloc(NetPacket.GetHeaderSize(packet.Property) + ReceivedSize);
            resultingPacket.Property = packet.Property;
            resultingPacket.DeliveryMethod = DeliveryMethod;

            int resultHeaderSize = NetPacket.GetHeaderSize(Fragments[0].Property);
            int firstFragmentSize = Fragments[0].Size - NetPacket.FragmentedHeaderTotalSize;
            for (int i = 0; i < ReceivedCount; i++)
            {
                var fragment = Fragments[i];

                Buffer.BlockCopy(
                    fragment.RawData,
                    NetPacket.FragmentedHeaderTotalSize,
                    resultingPacket.RawData,
                    resultHeaderSize + firstFragmentSize * i,
                    fragment.Size - NetPacket.FragmentedHeaderTotalSize);

                NetPool.PacketPool.Free(fragment);
            }

            Array.Clear(Fragments, 0, ReceivedCount);

            return resultingPacket;
        }
    }
}
