using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace EuNet.Core
{
    public class UdpSocketEx : UdpSocket
    {
        private ConcurrentDictionary<IPEndPoint, ISession> _channelMap;
        private NetPacket _cachedReceivedPacket;

        private readonly Func<byte[], int, NetPacket, IPEndPoint, bool> _onReceivedPacket;

        public UdpSocketEx(ILogger logger, Func<byte[], int, NetPacket, IPEndPoint, bool> onReceivedPacket)
            : base(logger)
        {
            _channelMap = new ConcurrentDictionary<IPEndPoint, ISession>();
            _cachedReceivedPacket = new NetPacket();
            _onReceivedPacket = onReceivedPacket;
        }

        internal bool TryGetSession(IPEndPoint endPoint, out ISession session)
        {
            return _channelMap.TryGetValue(endPoint, out session);
        }

        internal void AddSession(ISession session)
        {
            _channelMap[session.UdpChannel.PunchedEndPoint] = session;
        }

        internal void AddSession(ISession session, IPEndPoint ep)
        {
            _channelMap[ep] = session;
        }

        internal bool RemoveSession(ISession session, bool isAll = false)
        {
            if (session == null)
                return false;

            if (isAll)
            {
                _channelMap.RemoveAllByValue(session);
            }
            else
            {
                if (session.UdpChannel.PunchedEndPoint == null)
                    return false;

                ISession removedSession;
                _channelMap.TryRemove(session.UdpChannel.PunchedEndPoint, out removedSession);
                if (session == removedSession)
                    return true;
            }

            return false;
        }

        protected override void OnReceivedData(byte[] data, int size, SocketError error, IPEndPoint endPoint)
        {
            _cachedReceivedPacket.RawData = data;

            if (_onReceivedPacket(data, size, _cachedReceivedPacket, endPoint) == false)
            {
                ISession session;
                if (TryGetSession(endPoint, out session) == true)
                {
                    session?.UdpChannel?.OnReceivedRawUdpData(data, size, _cachedReceivedPacket, error, endPoint);
                }
            }
        }
    }
}
