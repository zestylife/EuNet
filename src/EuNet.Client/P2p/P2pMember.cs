using EuNet.Core;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EuNet.Client
{
    public enum P2pConnectState : byte
    {
        //! 홀펀칭 중
        HolePunching,

        //! 연결되지 않음 ( 홀펀칭 실패 시 이 상태로 되며 릴레이 서버를 통해서 통신한다. 주기적으로 다시 홀펀칭할수도 있다 )
        NotConnected,

        //! 직접 연결되었음 ( 내가 보낸 메시지가 상대편에만 잘 도착함 )
        Connected,

        //! 직접 연결되었음 ( 상대방과 나 모두 직접 전송 가능 )
        BothConnected,
    }

    public class P2pMember : P2pMemberBase
    {
        private readonly P2pGroup _p2pGroup;
        public P2pGroup P2pGroup => _p2pGroup;

        private P2pConnectState _state;
        public P2pConnectState State => _state;

        private int _holePunchingElapsedTime;
        private int _holePunchingStartCount;

        protected ILogger Logger { get; }

        public P2pMember(P2pGroup p2pGroup)
        {
            _p2pGroup = p2pGroup;
            Logger = p2pGroup.NetClient.LoggerFactory.CreateLogger(nameof(P2pMember));
        }

        internal void SetState(P2pConnectState state)
        {
            Logger.LogInformation($"SetState : {_state} to {state}");

            _state = state;
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

        public bool IsMine()
        {
            return Session.SessionId == P2pGroup.NetClient.SessionId;
        }

        public bool IsMaster()
        {
            return Session.SessionId == P2pGroup.MasterSessionId;
        }

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
