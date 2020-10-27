using System;
using System.Collections.Generic;
using System.Threading;

namespace EuNet.Core
{
    internal sealed class SequencedChannel : ChannelBase
    {
        private readonly Queue<NetPacket> _sendQueue;
        private int _localSequence;
        private ushort _remoteSequence;
        private readonly bool _reliable;
        private NetPacket _lastPacket;
        private readonly NetPacket _ackPacket;
        private bool _mustSendAck;
        private long _lastPacketSendTime;

        private readonly UdpChannel _udpChannel;
        private readonly DeliveryMethod _deliveryMethod;

        public SequencedChannel(IChannelOption channelOption, ILogger logger, NetStatistic statistic, UdpChannel udpChannel, bool reliable)
            : base(channelOption, logger, statistic)
        {
            _sendQueue = new Queue<NetPacket>(64);
            _udpChannel = udpChannel;
            _reliable = reliable;

            if (_reliable)
            {
                _deliveryMethod = DeliveryMethod.ReliableSequenced;

                _ackPacket = new NetPacket(PacketProperty.Ack, 0);
                _ackPacket.DeliveryMethod = _deliveryMethod;
            }
            else
            {
                _deliveryMethod = DeliveryMethod.Sequenced;
            }
        }

        public override void Init(CancellationTokenSource cts)
        {
            base.Init(cts);

            lock (_sendQueue)
            {
                while (_sendQueue.Count > 0)
                    NetPool.PacketPool.Free(_sendQueue.Dequeue());
            }
        }

        public override void Close()
        {
            base.Close();

            lock (_sendQueue)
            {
                while (_sendQueue.Count > 0)
                    NetPool.PacketPool.Free(_sendQueue.Dequeue());
            }
        }

        public override bool Update(int elapsedTime)
        {
            return SendPendingPacket();
        }

        public override void SendAsync(NetPacket poolingPacket)
        {
            if (_reliable)
                Interlocked.Increment(ref _statistic.UdpReliablePacketSentCount);

            lock (_sendQueue)
                _sendQueue.Enqueue(poolingPacket);
        }

        public bool SendPendingPacket()
        {
            if (_reliable && _sendQueue.Count == 0)
            {
                long currentTime = DateTime.UtcNow.Ticks;
                long packetHoldTime = currentTime - _lastPacketSendTime;
                if (packetHoldTime < _udpChannel.ResendDelay * TimeSpan.TicksPerMillisecond)
                    return true;

                var packet = _lastPacket;
                if (packet != null)
                {
                    _lastPacketSendTime = currentTime;
                    _udpChannel.SendTo(packet.RawData, 0, packet.Size, UdpChannel.SendMode.Buffered);
                }
            }
            else
            {
                lock (_sendQueue)
                {
                    while (_sendQueue.Count > 0)
                    {
                        NetPacket packet = _sendQueue.Dequeue();
                        _localSequence = (_localSequence + 1) % NetPacket.MaxSequence;
                        packet.Sequence = (ushort)_localSequence;

                        _udpChannel.SendTo(packet.RawData, 0, packet.Size, UdpChannel.SendMode.Buffered);

                        if (_reliable && _sendQueue.Count == 0)
                        {
                            _lastPacketSendTime = DateTime.UtcNow.Ticks;
                            _lastPacket = packet;
                        }
                        else
                        {
                            NetPool.PacketPool.Free(packet);
                        }
                    }
                }
            }

            if (_reliable && _mustSendAck)
            {
                _mustSendAck = false;
                _ackPacket.Sequence = _remoteSequence;

                _udpChannel.SendTo(_ackPacket.RawData, 0, _ackPacket.Size, UdpChannel.SendMode.Buffered);
            }

            return true;
        }

        public bool ProcessPacket(NetPacket packet)
        {
            if (packet.IsFragmented)
                return false;

            if (packet.Property == PacketProperty.Ack)
            {
                if (_reliable && _lastPacket != null && packet.Sequence == _lastPacket.Sequence)
                    _lastPacket = null;
                return false;
            }

            int relative = NetUtil.RelativeSequenceNumber(packet.Sequence, _remoteSequence);
            bool packetProcessed = false;

            if (packet.Sequence < NetPacket.MaxSequence && relative > 0)
            {
                if (_reliable)
                    Interlocked.Add(ref _statistic.UdpPacketLossCount, (relative - 1));

                _remoteSequence = packet.Sequence;

                _udpChannel.PacketReceived?.Invoke(_udpChannel, packet);

                packetProcessed = true;
            }

            _mustSendAck = true;
            return packetProcessed;
        }
    }
}
