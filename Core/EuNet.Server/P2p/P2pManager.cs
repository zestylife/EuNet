using EuNet.Core;
using System.Collections.Generic;

namespace EuNet.Server
{
    public class P2pManager
    {
        //! P2P 그룹 맵
        private Dictionary<ushort, P2pGroup> _p2pGroupMap;

        //! 마지막으로 발급된 그룹 아이디
        private ushort _lastGenerateId = 0x8000;

        public P2pManager()
        {
            _p2pGroupMap = new Dictionary<ushort, P2pGroup>();
        }

        public void Clear()
        {
            _p2pGroupMap.Clear();
        }

        //! 새로운 P2P그룹을 생성한다
        public P2pGroup CreateP2pGroup()
        {
            lock (_p2pGroupMap)
            {
                if (_lastGenerateId == ushort.MaxValue)
                    return null;

                var id = GenerateId();
                P2pGroup p2pGroup = new P2pGroup(id);

                _p2pGroupMap.Add(p2pGroup.Id, p2pGroup);

                return p2pGroup;
            }
        }

        //! P2P 그룹을 파괴한다
        public bool DestroyP2pGroup(ushort groupSessionKey)
        {
            P2pGroup p2pGroup = Find(groupSessionKey);
            if (p2pGroup == null)
                return false;

            // P2P 내부 인원을 모두 내보낸다
            p2pGroup.LeaveAll();

            lock (_p2pGroupMap)
            {
                _p2pGroupMap.Remove(groupSessionKey);
            }

            return true;
        }

        public P2pGroup Find(ushort groupSessionKey)
        {
            lock (_p2pGroupMap)
            {
                P2pGroup p2pGroup = null;
                if (_p2pGroupMap.TryGetValue(groupSessionKey, out p2pGroup) == true)
                    return p2pGroup;
            }

            return null;
        }

        private ushort GenerateId()
        {
            lock (_p2pGroupMap)
            {
                while (true)
                {
                    var id = _lastGenerateId++;
                    if (_lastGenerateId >= ushort.MaxValue)
                        _lastGenerateId = 0x8000;

                    if (_p2pGroupMap.ContainsKey(id))
                        continue;

                    return id;
                }
            }
        }

        internal void OnSessionClose(ISession session)
        {
#warning 추후 최적화를 하자
            ServerSession s = session as ServerSession;

            lock (_p2pGroupMap)
            {
                foreach (var kvp in _p2pGroupMap)
                {
                    try
                    {
                        kvp.Value.Leave(s);
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
