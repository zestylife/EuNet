using System.Collections.Generic;

namespace EuNet.Core
{
    internal class PacketFragments
    {
        // 보낼때 사용하는 아이디
        private ushort _fragmentId;
        private object _fragmentObject;

        // 받을때 사용하는 맵
        private readonly Dictionary<ushort, FragmentInfo> _holdedFragments;

        public PacketFragments()
        {
            _fragmentObject = new object();
            _holdedFragments = new Dictionary<ushort, FragmentInfo>();
        }

        public void Init()
        {
            _fragmentId = 0;
            _holdedFragments.Clear();
        }

        public void Clear()
        {
            Init();
        }

        public ushort GenerateId()
        {
            lock (_fragmentObject)
            {
                return _fragmentId++;
            }
        }

        public NetPacket AddFragment(NetPacket packet)
        {
            ushort packetFragId = packet.FragmentId;
            FragmentInfo fragmentInfo;
            if (_holdedFragments.TryGetValue(packetFragId, out fragmentInfo) == false)
            {
                fragmentInfo = new FragmentInfo(packet);
                _holdedFragments.Add(packetFragId, fragmentInfo);
            }

            var resultPacket = fragmentInfo.AddFragment(packet);
            if (resultPacket != null)
            {
                // 처리가 완료되었으니 빼자!
                _holdedFragments.Remove(packetFragId);
            }

            return resultPacket;
        }
    }
}
