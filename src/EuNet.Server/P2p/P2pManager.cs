using EuNet.Core;
using System.Collections.Generic;

namespace EuNet.Server
{
    /// <summary>
    /// P2P 그룹을 관리하는 매니저
    /// </summary>
    public class P2pManager
    {
        private Dictionary<ushort, P2pGroup> _p2pGroupMap;

        /// <summary>
        /// 마지막으로 발급된 그룹 아이디
        /// </summary>
        private ushort _lastGenerateId = 0x8000;

        public P2pManager()
        {
            _p2pGroupMap = new Dictionary<ushort, P2pGroup>();
        }

        /// <summary>
        /// 모든 그룹을 제거함
        /// </summary>
        public void Clear()
        {
            _p2pGroupMap.Clear();
        }

        /// <summary>
        /// 새로운 P2P 그룹을 생성함
        /// </summary>
        /// <returns>생성된 P2P 그룹</returns>
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

        /// <summary>
        /// P2P 그룹을 파괴함
        /// </summary>
        /// <param name="groupId">P2P 그룹 아이디</param>
        /// <returns>성공여부</returns>
        public bool DestroyP2pGroup(ushort groupId)
        {
            P2pGroup p2pGroup = Find(groupId);
            if (p2pGroup == null)
                return false;

            // P2P 내부 인원을 모두 내보낸다
            p2pGroup.LeaveAll();

            lock (_p2pGroupMap)
            {
                _p2pGroupMap.Remove(groupId);
            }

            return true;
        }

        /// <summary>
        /// P2P 그룹 아이디로 P2P 그룹을 찾음
        /// </summary>
        /// <param name="groupId">찾을 P2P 그룹 아이디</param>
        /// <returns>찾은 P2P 그룹. 없다면 null</returns>
        public P2pGroup Find(ushort groupId)
        {
            lock (_p2pGroupMap)
            {
                P2pGroup p2pGroup = null;
                if (_p2pGroupMap.TryGetValue(groupId, out p2pGroup) == true)
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
