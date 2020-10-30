using EuNet.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EuNet.Unity")]
namespace EuNet.Client
{
    //! 클라이언트에서의 P2P 그룹. 여기에 자기 자신도 포함된다
    public class P2pGroup
    {
        private readonly NetClient _netClient;
        public NetClient NetClient => _netClient;

        private ushort _id;

        //! 그룹에 소속된 세션들
        private LinkedList<P2pMember> _memberList;

        //! 마스터 세션
        private ushort _masterSessionId;

        protected ILogger Logger { get; }

        internal P2pGroup(NetClient netClient, ushort id, ushort masterSessionId)
        {
            _netClient = netClient;
            _id = id;
            _masterSessionId = masterSessionId;
            _memberList = new LinkedList<P2pMember>();
            Logger = _netClient.LoggerFactory.CreateLogger(nameof(P2pGroup));
        }

        public ushort Id => _id;
        public LinkedList<P2pMember> MemberList => _memberList;
        public ushort MasterSessionId => _masterSessionId;

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

        public P2pMember GetMasterMember()
        {
            return Find(_masterSessionId);
        }

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

        //! 내가 마스터인가?
        public bool MasterIsMine()
        {
            if (_masterSessionId == _netClient.SessionId)
                return true;

            return false;
        }
    }
}
