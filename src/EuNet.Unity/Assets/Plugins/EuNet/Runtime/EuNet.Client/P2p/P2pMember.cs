using EuNet.Core;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EuNet.Client
{
    /// <summary>
    /// P2P 멤버 클래스
    /// 홀펀칭, 데이터 전송 등의 기능을 함
    /// 자기 자신의 경우 Session을 NetClient 로 등록됨
    /// 다른 유저의 경우 Session을 P2pSession 으로 등록됨
    /// </summary>
    public class P2pMember : P2pMemberBase
    {
        private readonly P2pGroup _p2pGroup;
        private P2pConnectState _state;
        private int _holePunchingElapsedTime;
        private int _holePunchingStartCount;

        protected ILogger Logger { get; }

        /// <summary>
        /// 소속된 P2P 그룹
        /// </summary>
        public P2pGroup P2pGroup => _p2pGroup;

        /// <summary>
        /// 현재 연결 상태
        /// </summary>
        public P2pConnectState State => _state;

        public P2pMember(P2pGroup p2pGroup)
        {
            _p2pGroup = p2pGroup;
            Logger = p2pGroup.NetClient.LoggerFactory.CreateLogger(nameof(P2pMember));
        }

        internal void SetState(P2pConnectState state)
        {
            Logger.LogInformation($"SetState : {_state} to {state}");

            _state = state;

            if(Session.UdpChannel != null)
            {
                var isConnected = IsConnected();

                Session.UdpChannel.IsRunPing = isConnected;
                Session.UdpChannel.IsRunMtu = isConnected;
            }
        }

        internal void Update(int elasepdTime)
        {
            if (IsMine() == true)
                return;

            if (State == P2pConnectState.HolePunching)
            {
                // 연결이 안되었다면 홀펀칭 시도를 하자
                _holePunchingElapsedTime += elasepdTime;

                if (_holePunchingElapsedTime >= 200)
                {
                    _holePunchingElapsedTime = 0;

                    if (_holePunchingStartCount < 60)
                    {
                        SendHolePunchingStart();
                    }
                    else
                    {
                        SetState(P2pConnectState.NotConnected);
                    }
                }
            }

            Session?.Update(elasepdTime);
        }

        private void SendHolePunchingStart()
        {
            if (P2pGroup.NetClient.ClientOption.IsForceRelay)
                return;

            IPEndPoint ep = null;

            switch (_holePunchingStartCount % 3)
            {
                case 0:
                    ep = Session.UdpChannel.LocalEndPoint;
                    break;
                case 1:
                    ep = Session.UdpChannel.RemoteEndPoint;
                    break;
                case 2:
                    ep = Session.UdpChannel.TempEndPoint;
                    break;
            }

            _holePunchingStartCount++;

            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write(ep);

                NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.HolePunchingStart, writer);

                try
                {
                    packet.SessionIdForConnection = P2pGroup.NetClient.SessionId;
                    packet.DeliveryMethod = DeliveryMethod.Unreliable;
                    SocketError error = SocketError.Success;
                    P2pGroup.NetClient.UdpSocket.SendTo(packet.RawData, 0, packet.Size, ep, ref error);

                    Logger.LogInformation($"SendHolePunchingStart to {ep}");
                }
                finally
                {
                    NetPool.PacketPool.Free(packet);
                }
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }

        /// <summary>
        /// 이 멤버가 나 자신인지 여부
        /// </summary>
        public bool IsMine()
        {
            return Session.SessionId == P2pGroup.NetClient.SessionId;
        }

        /// <summary>
        /// 이 멤버가 마스터인지 여부
        /// </summary>
        public bool IsMaster()
        {
            return Session.SessionId == P2pGroup.MasterSessionId;
        }

        /// <summary>
        /// 단방향 또는 양뱡향으로 연결되었는지 여부.
        /// </summary>
        public bool IsConnected()
        {
            if (_state == P2pConnectState.Connected || _state == P2pConnectState.BothConnected)
                return true;

            return false;
        }

        internal void ViewNotification(byte[] data, int offset, int length, int viewId, DeliveryMethod deliveryMethod)
        {
            if (deliveryMethod == DeliveryMethod.Tcp)
                throw new Exception("Not support p2p tcp delivery");

            Session?.SessionRequest.ViewNotification(data, offset, length, viewId, deliveryMethod);
        }

        internal Task<NetDataBufferReader> ViewRequestAsync(byte[] data, int offset, int length, int viewId, DeliveryMethod deliveryMethod, TimeSpan? timeout)
        {
            if (deliveryMethod == DeliveryMethod.Tcp)
                throw new Exception("Not support p2p tcp delivery");

            return Session?.SessionRequest.ViewRequestAsync(data, offset, length, viewId, deliveryMethod, timeout);
        }
    }
}
