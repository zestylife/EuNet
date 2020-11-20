using EuNet.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EuNet.Unity")]
namespace EuNet.Client
{
    /// <summary>
    /// P2P 그룹 클래스.
    /// 자기 자신도 멤버로 포함된다.
    /// </summary>
    public class P2pGroup
    {
        private readonly NetClient _netClient;
        public NetClient NetClient => _netClient;
        
        private ushort _id;
        private LinkedList<P2pMember> _memberList;
        private ushort _masterSessionId;
        protected ILogger Logger { get; }

        /// <summary>
        /// 그룹 고유 아이디
        /// </summary>
        public ushort Id => _id;

        /// <summary>
        /// 그룹에 소속된 멤버들
        /// </summary>
        public LinkedList<P2pMember> MemberList => _memberList;

        /// <summary>
        /// 마스터 세션 아이디
        /// </summary>
        public ushort MasterSessionId => _masterSessionId;

        internal P2pGroup(NetClient netClient, ushort id, ushort masterSessionId)
        {
            _netClient = netClient;
            _id = id;
            _masterSessionId = masterSessionId;
            _memberList = new LinkedList<P2pMember>();
            Logger = _netClient.LoggerFactory.CreateLogger(nameof(P2pGroup));
        }

        internal void Close()
        {
            foreach (var member in _memberList)
            {
                if (member.IsMine())
                    continue;

                member.Close();
            }

            _memberList.Clear();
        }

        internal P2pMember Join(ISession session, ushort masterSessionId)
        {
            if (Contains(session.SessionId) == true)
            {
                Logger.LogError($"Already exist P2p member : {session.SessionId}");
                return null;
            }

            _masterSessionId = masterSessionId;

            P2pMember p2pMember = new P2pMember(this);
            p2pMember.SetSession(session);

            _memberList.AddLast(p2pMember);

            Logger.LogInformation($"Join p2p : {session.SessionId}  Master : {masterSessionId}");

            return p2pMember;
        }

        internal P2pMember Leave(ushort sessionId, ushort masterSessionId)
        {
            P2pMember member = Find(sessionId);
            if (member == null)
            {
                Logger.LogError($"Not exist P2p member : {sessionId}");
                return null;
            }

            _memberList.Remove(member);

            _masterSessionId = masterSessionId;

            Logger.LogInformation($"Leave p2p : {sessionId}  Master : {masterSessionId}");

            return member;
        }

        /// <summary>
        /// 모든 멤버에게 데이터를 전송함
        /// </summary>
        /// <param name="data">보낼 데이터 버퍼</param>
        /// <param name="offset">보낼 데이터 버퍼 오프셋</param>
        /// <param name="length">보낼 데이터 길이</param>
        /// <param name="deliveryMethod">전송 방법</param>
        public void SendAll(byte[] data, int offset, int length, DeliveryMethod deliveryMethod)
        {
            if (deliveryMethod == DeliveryMethod.Tcp)
                throw new Exception("Not support p2p tcp delivery");

            foreach (var member in _memberList)
            {
                if (member.IsMine() == true)
                    continue;

                member.SendAsync(data, offset, length, deliveryMethod);
            }
        }

        /// <summary>
        /// 모든 멤버에게 데이터를 전송함
        /// </summary>
        /// <param name="dataWriter">보낼 데이터를 가지고 있는 NetDataWriter</param>
        /// <param name="deliveryMethod">전송 방법</param>
        public void SendAll(NetDataWriter dataWriter, DeliveryMethod deliveryMethod)
        {
            SendAll(dataWriter.Data, 0, dataWriter.Length, deliveryMethod);
        }

        internal void ViewNotification(byte[] data, int offset, int length, int viewId, DeliveryMethod deliveryMethod)
        {
            if (deliveryMethod == DeliveryMethod.Tcp)
                throw new Exception("Not support p2p tcp delivery");

            foreach (var member in _memberList)
            {
                if (member.IsMine() == true)
                    continue;

                member.ViewNotification(data, offset, length, viewId, deliveryMethod);
            }
        }

        internal void SetMaster(ushort masterSessionId)
        {
            Logger.LogInformation($"Change p2p master {_masterSessionId} to {masterSessionId}");

            _masterSessionId = masterSessionId;
        }

        /// <summary>
        /// 마스터 멤버를 가져온다
        /// </summary>
        /// <returns>마스터 멤버</returns>
        public P2pMember GetMasterMember()
        {
            return Find(_masterSessionId);
        }

        /// <summary>
        /// 세션 아이디로 세션이 있는지 확인
        /// </summary>
        /// <param name="sessionId">세션 아이디</param>
        /// <returns>포함여부</returns>
        public bool Contains(ushort sessionId)
        {
            LinkedListNode<P2pMember> node = _memberList.First;

            while (node != null)
            {
                P2pMember member = node.Value;
                node = node.Next;

                if (member != null && member.SessionId == sessionId)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 세션 아이디로 멤버를 구한다
        /// </summary>
        /// <param name="sessionId">세션 아이디</param>
        /// <returns>찾은 멤버</returns>
        public P2pMember Find(ushort sessionId)
        {
            LinkedListNode<P2pMember> node = _memberList.First;

            while (node != null)
            {
                P2pMember member = node.Value;
                node = node.Next;

                if (member != null && member.SessionId == sessionId)
                    return member;
            }

            return null;
        }

        /// <summary>
        /// 모든 멤버를 리스트로 가져옵니다
        /// </summary>
        /// <returns>모든 멤버 리스트</returns>
        public List<P2pMember> GetList()
        {
            return new List<P2pMember>(_memberList);
        }

        internal void Update(int elapsedTime)
        {
            LinkedListNode<P2pMember> node = _memberList.First;

            while (node != null)
            {
                P2pMember member = node.Value;
                node = node.Next;

                member.Update(elapsedTime);
            }
        }

        /// <summary>
        /// 나 자신이 마스터인지 여부
        /// </summary>
        public bool MasterIsMine()
        {
            if (_masterSessionId == _netClient.SessionId)
                return true;

            return false;
        }
    }
}
