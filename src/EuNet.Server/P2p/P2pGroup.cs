using EuNet.Core;
using System.Collections.Generic;

namespace EuNet.Server
{
    /// <summary>
    /// P2P 그룹
    /// </summary>
    public class P2pGroup
    {
        /// <summary>
        /// P2P그룹 아이디
        /// </summary>
        private ushort _id;

        /// <summary>
        /// 마스터 세션 아이디
        /// </summary>
        private ushort _masterSessionId;

        /// <summary>
        /// 그룹에 속한 멤버 리스트
        /// </summary>
        private LinkedList<P2pMember> _memberList;

        /// <summary>
        /// 멤버의 수
        /// </summary>
        public int MemberCount
        {
            get
            {
                lock (_memberList)
                    return _memberList.Count;
            }
        }

        /// <summary>
        /// P2P 그룹 아이디
        /// </summary>
        public ushort Id => _id;

        /// <summary>
        /// 마스터 세션 아이디
        /// </summary>
        public ushort MasterSessionId => _masterSessionId;

        public P2pGroup(ushort id)
        {
            _id = id;
            _masterSessionId = ushort.MaxValue;

            _memberList = new LinkedList<P2pMember>();
        }

        /// <summary>
        /// 세션을 P2P에 가입시킴
        /// </summary>
        /// <param name="session">가입시킬 세션</param>
        /// <returns>성공여부</returns>
        public bool Join(ServerSession session)
        {
            if (Contains(session.SessionId) == true)
                return false;

            var p2pMember = new P2pMember();

            try
            {
                lock (_memberList)
                {
                    if (_memberList.Count == 0)
                    {
                        _masterSessionId = session.SessionId;
                    }

                    p2pMember.SetSession(session);
                    _memberList.AddLast(p2pMember);

                    p2pMember.SendJoinP2pGroup(
                        Id,
                        session.SessionId,
                        MasterSessionId,
                        session.UdpChannel.RemoteEndPoint,
                        session.UdpChannel.LocalEndPoint);

                    foreach (var member in _memberList)
                    {
                        if (member.Session == session)
                            continue;

                        // 나한테 다른 유저 정보를 보내자
                        p2pMember.SendJoinP2pGroup(
                            Id,
                            member.SessionId,
                            _masterSessionId,
                            member.Session.UdpChannel.RemoteEndPoint,
                            member.Session.UdpChannel.LocalEndPoint);

                        // 다른사람한테 내정보를 보내자
                        member.SendJoinP2pGroup(
                            Id,
                            session.SessionId,
                            _masterSessionId,
                            session.UdpChannel.RemoteEndPoint,
                            session.UdpChannel.LocalEndPoint);
                    }
                }
            }
            catch
            {
                return false;
            }

            //Console.WriteLine("Join P2P Group : {0} {1} {2}", m_groupSessionKey, session.GetSessionKey(), m_masterSessionKey);

            return true;

        }

        /// <summary>
        /// 세션을 P2P그룹에서 제거함
        /// </summary>
        /// <param name="session">제거할 세션</param>
        /// <returns>성공여부</returns>
        public bool Leave(ServerSession session)
        {
            var member = Find(session.SessionId);
            if (member == null)
                return false;

            lock (_memberList)
            {
                try
                {
                    if (session.SessionId == _masterSessionId)
                    {
                        if (_memberList.Count > 1)
                        {
                            ChangeMaster(false, false);
                        }
                    }

                    foreach (var m in _memberList)
                    {
                        m.SendLeaveP2pGroup(
                            Id,
                            member.Session.SessionId,
                            _masterSessionId);
                    }
                }
                finally
                {
                    _memberList.Remove(member);
                    member = null;
                }

                if (_memberList.Count == 0)
                {
                    _masterSessionId = ushort.MaxValue;
                }
            }

            //Console.WriteLine("Leave P2P Group : {0} {1} {2}", m_groupSessionKey, session.GetSessionKey(), m_masterSessionKey);

            return true;
        }

        /// <summary>
        /// 모든 멤버를 제거함
        /// </summary>
        public void LeaveAll()
        {
            lock (_memberList)
            {
                _masterSessionId = ushort.MaxValue;

                foreach (P2pMember member in _memberList)
                {
                    member.SendLeaveP2pGroup(
                        Id,
                        member.Session.SessionId,
                        _masterSessionId);
                }

                _memberList.Clear();
            }
        }

        /// <summary>
        /// 특정 세션아이디가 멤버에 포함되어 있는지 여부
        /// </summary>
        /// <param name="sessionId">세션아이디</param>
        /// <returns>멤버에 포함되어 있는지 여부</returns>
        public bool Contains(ushort sessionId)
        {
            lock (_memberList)
            {
                foreach (P2pMember member in _memberList)
                {
                    if (member.SessionId == sessionId)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 특정 세션아이디로 멤버를 구함
        /// </summary>
        /// <param name="sessionId">세션아이디</param>
        /// <returns>찾은 P2P멤버. 없다면 null</returns>
        public P2pMember Find(ushort sessionId)
        {
            lock (_memberList)
            {
                foreach (P2pMember member in _memberList)
                {
                    if (member.SessionId == sessionId)
                        return member;
                }
            }

            return null;
        }

        /// <summary>
        /// P2P 마스터를 자동으로 변경함.
        /// MasterPriority 와 Ping 을 평가하여 가장 적합한 마스터를 선택하여 변경함
        /// </summary>
        /// <param name="containCurrentMaster">현재 마스터도 포함하는지 여부</param>
        /// <param name="sendInfo">변경 정보를 멤버들에게 전송할지 여부</param>
        public void ChangeMaster(bool containCurrentMaster, bool sendInfo)
        {
            // 마스터를 변경한다
            P2pMember master = null;
            ushort prevMasterSessionKey = _masterSessionId;
            sbyte priority = sbyte.MinValue;
            int bestPing = int.MaxValue;

            lock (_memberList)
            {
                foreach (P2pMember member in _memberList)
                {
                    if (containCurrentMaster == false &&
                        prevMasterSessionKey == member.Session.SessionId)
                        continue;
                    
                    if (priority == sbyte.MinValue)
                    {
                        master = member;
                        priority = member.MasterPriority;
                        bestPing = member.Ping;
                        continue;
                    }

                    if (member.MasterPriority > priority ||
                        (member.MasterPriority == priority && (ushort)member.Ping < bestPing))
                    {
                        master = member;
                        priority = member.MasterPriority;
                        bestPing = member.Ping;
                    }
                }
            }

            if (master != null)
            {
                if (sendInfo == true)
                {
                    SetMaster(master.SessionId);
                }
                else
                {
                    _masterSessionId = master.SessionId;
                }

                //Console.WriteLine("SetMaster : Session[{0}] Ping[{1}] PickType[{2}]", master.m_session.GetSessionKey(), bestPing, pickType);
            }
        }

        /// <summary>
        /// 특정 세션을 마스터로 지정함
        /// </summary>
        /// <param name="sessionId">마스터로 지정할 세션아이디</param>
        public void SetMaster(ushort sessionId)
        {
            if (_masterSessionId == sessionId)
                return;

            if (Contains(sessionId) == false)
                return;

            _masterSessionId = sessionId;

            lock (_memberList)
            {
                foreach (var member in _memberList)
                {
                    member.SendChangeMasterP2pGroup(
                        Id,
                        _masterSessionId);
                }
            }
        }

        /// <summary>
        /// 멤버들에게 데이터를 전송함
        /// </summary>
        /// <param name="writer">전송할 데이터가 있는 NetDataWriter</param>
        /// <param name="deliveryMethod">전송 방식</param>
        /// <param name="exceptSessionId">제외할 세션아이디. ushort.MaxValue 일 경우 제외하지 않음</param>
        public void Send(NetDataWriter writer, DeliveryMethod deliveryMethod, ushort exceptSessionId = ushort.MaxValue)
        {
            lock (_memberList)
            {
                foreach (var member in _memberList)
                {
                    if (member.SessionId == exceptSessionId)
                        continue;

                    member.SendAsync(writer, deliveryMethod);
                }
            }
        }

        /// <summary>
        /// 특정 멤버에게 데이터를 전송
        /// </summary>
        /// <param name="writer">전송할 데이터가 있는 NetDataWriter</param>
        /// <param name="deliveryMethod">전송 방식</param>
        /// <param name="sessionId">전송할 세션 아이디</param>
        /// <returns>전송여부</returns>
        public bool SendToSession(NetDataWriter writer, DeliveryMethod deliveryMethod, ushort sessionId)
        {
            lock (_memberList)
            {
                foreach (var member in _memberList)
                {
                    if (member.SessionId == sessionId)
                    {
                        member.SendAsync(writer, deliveryMethod);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 마스터에게 데이터를 전송
        /// </summary>
        /// <param name="writer">전송할 데이터가 있는 NetDataWriter</param>
        /// <param name="deliveryMethod">전송 방식</param>
        public void SendToMaster(NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            SendToSession(writer, deliveryMethod, _masterSessionId);
        }
    }
}
