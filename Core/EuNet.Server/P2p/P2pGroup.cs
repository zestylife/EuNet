using EuNet.Core;
using System.Collections.Generic;

namespace EuNet.Server
{
    public class P2pGroup
    {
        //! P2P그룹 아이디
        private ushort _id;

        //! 마스터 세션
        private ushort _masterSessionId;

        //! 그룹에 소속된 세션들
        private LinkedList<P2pMember> _memberList;

        public int MemberCount
        {
            get
            {
                lock (_memberList)
                    return _memberList.Count;
            }
        }

        public P2pGroup(ushort id)
        {
            _id = id;
            _masterSessionId = ushort.MaxValue;

            _memberList = new LinkedList<P2pMember>();
        }

        public ushort Id => _id;
        public ushort MasterSessionKey => _masterSessionId;

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
                        MasterSessionKey,
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

        public void SendToSession(NetDataWriter writer, DeliveryMethod deliveryMethod, ushort sessionId)
        {
            lock (_memberList)
            {
                foreach (var member in _memberList)
                {
                    if (member.SessionId == sessionId)
                    {
                        member.SendAsync(writer, deliveryMethod);
                        break;
                    }
                }
            }
        }

        public void SendToMaster(NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            SendToSession(writer, deliveryMethod, _masterSessionId);
        }
    }
}
