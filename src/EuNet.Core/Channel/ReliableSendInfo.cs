using System;
using System.Threading;

namespace EuNet.Core
{
    public class ReliableSendInfo
    {
        private NetPacket _packet;
        private long _createTime;
        private long _lastSentTime;
        private bool _isSent;

        public ReliableSendInfo()
        {

        }

        public NetPacket Packet => _packet;
        public long CreateTime => _createTime;
        public long LastSentTime
        {
            get { return _lastSentTime; }
            set { _lastSentTime = value; }
        }

        public void Init(NetPacket packet, long createTime)
        {
            _packet = packet;
            _createTime = createTime;
            _lastSentTime = 0;
            _isSent = false;
        }

        public void Clear()
        {
            if (_packet != null)
            {
                NetPool.PacketPool.Free(_packet);
                _packet = null;
            }
        }

        public bool TrySend(long currentTime, int disconnectTimeoutMs, UdpChannel udpChannel, NetStatistic statistic)
        {
            if (_packet == null)
                return true;

            if (_isSent)
            {
                if (currentTime >= _createTime + disconnectTimeoutMs * TimeSpan.TicksPerMillisecond)
                    return false;

                long resendDelay = udpChannel.ResendDelay * TimeSpan.TicksPerMillisecond;
                long packetHoldTime = currentTime - _lastSentTime;
                if (packetHoldTime < resendDelay)
                    return true;

                Interlocked.Increment(ref statistic.UdpResentCount);
            }

            _lastSentTime = currentTime;
            _isSent = true;

            udpChannel?.SendTo(_packet.RawData, 0, _packet.Size, UdpChannel.SendMode.Buffered);

            return true;
        }
    }
}
