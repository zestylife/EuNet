//#define RELIABLE_LOG

using System;
using System.Collections.Generic;
using System.Threading;

namespace EuNet.Core
{
    public sealed class ReliableChannel : ChannelBase
    {
        private readonly Queue<NetPacket> _sendQueue;
        private readonly NetPacket _ackPacket;
        private readonly ReliableSendInfo[] _pendingPackets;
        private readonly NetPacket[] _receivedPackets;
        private readonly bool[] _earlyReceived;

        private int _localSequence;
        private int _remoteSequence;
        private int _localWindowStart;
        private int _remoteWindowStart;

        private volatile bool _sendAcks;

        private const int WindowSize = 64;
        private const int BitsInByte = 8;

        //! 스퀀스에 따라서 정렬하는가?
        private readonly bool _ordered;
        private readonly UdpChannel _udpChannel;
        private readonly DeliveryMethod _deliveryMethod;

        public ReliableChannel(IChannelOption channelOption, ILogger logger, NetStatistic statistic, UdpChannel channel, bool ordered)
            : base(channelOption, logger, statistic)
        {
            _udpChannel = channel;
            _sendQueue = new Queue<NetPacket>(WindowSize);

            _pendingPackets = new ReliableSendInfo[WindowSize];
            for (int i = 0; i < _pendingPackets.Length; i++)
            {
                _pendingPackets[i] = new ReliableSendInfo();
            }

            _ordered = ordered;

            if (_ordered)
            {
                _receivedPackets = new NetPacket[WindowSize];
                _deliveryMethod = DeliveryMethod.ReliableOrdered;
            }
            else
            {
                _earlyReceived = new bool[WindowSize];
                _deliveryMethod = DeliveryMethod.ReliableUnordered;
            }

            int bytesCount = (WindowSize - 1) / BitsInByte + 2;
            _ackPacket = new NetPacket(PacketProperty.Ack, bytesCount);
            _ackPacket.DeliveryMethod = _deliveryMethod;
        }

        public override void Init(CancellationTokenSource cts)
        {
            base.Init(cts);

            _localSequence = 0;
            _remoteSequence = 0;
            _localWindowStart = 0;
            _remoteWindowStart = 0;

            _sendAcks = false;

            lock (_ackPacket)
            {
                int headerSize = NetPacket.GetHeaderSize(PacketProperty.Ack);
                Array.Clear(_ackPacket.RawData, headerSize, _ackPacket.RawData.Length - headerSize);
            }

            ClearBuffer();
        }

        public override void Close()
        {
            base.Close();
        }

        internal override void OnClosed()
        {
            base.OnClosed();

            ClearBuffer();
        }

        private void ClearBuffer()
        {
            lock (_pendingPackets)
            {
                if (_ordered)
                {
                    for (int i = 0; i < _pendingPackets.Length; ++i)
                    {
                        _pendingPackets[i].Clear();

                        NetPool.PacketPool.Free(_receivedPackets[i]);
                        _receivedPackets[i] = null;
                    }
                }
                else
                {
                    for (int i = 0; i < _pendingPackets.Length; ++i)
                    {
                        _pendingPackets[i].Clear();

                        _earlyReceived[i] = false;
                    }
                }
            }

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
            Interlocked.Increment(ref _statistic.UdpReliablePacketSentCount);

            lock (_sendQueue)
                _sendQueue.Enqueue(poolingPacket);
        }

        public bool SendPendingPacket()
        {
            // 응답패킷을 보내야 한다면
            if (_sendAcks == true)
            {
                // lock에 비용이 많이드므로 먼저 체크한번 하고 나중에 또 체크하자
                lock (_ackPacket)
                {
                    if (_sendAcks == true)
                    {
                        _sendAcks = false;
                        _udpChannel.SendTo(_ackPacket.RawData, 0, _ackPacket.Size, UdpChannel.SendMode.Buffered);
                    }
                }
            }

            lock (_pendingPackets)
            {
                long nowTicks = DateTime.Now.Ticks;

                lock (_sendQueue)
                {
                    while (_sendQueue.Count > 0)
                    {
                        int relate = NetUtil.RelativeSequenceNumber(_localSequence, _localWindowStart);
                        if (relate >= WindowSize)
                            break;

                        NetPacket packet = _sendQueue.Dequeue();

                        packet.Sequence = (ushort)_localSequence;
                        _pendingPackets[_localSequence % WindowSize].Init(packet, nowTicks);
                        _localSequence = (_localSequence + 1) % NetPacket.MaxSequence;
                    }
                }

                for (int pendingSeq = _localWindowStart; pendingSeq != _localSequence; pendingSeq = (pendingSeq + 1) % NetPacket.MaxSequence)
                {
                    if (_pendingPackets[pendingSeq % WindowSize].TrySend(
                        nowTicks,
                        _channelOption.RudpDisconnectTimeout,
                        _udpChannel,
                        _statistic) == false)
                        return false;
                }
            }

            return true;
        }

        public bool ProcessPacket(NetPacket poolingPacket)
        {
            if (poolingPacket.Property == PacketProperty.Ack)
            {
                try
                {
                    ProcessAck(poolingPacket);
                }
                finally
                {
                    NetPool.PacketPool.Free(poolingPacket);
                }

                return false;
            }

            int seq = poolingPacket.Sequence;
            if (seq >= NetPacket.MaxSequence)
            {

#if RELIABLE_LOG
                Console.WriteLine($"rudp bad sequence {seq}");
#endif
                return false;
            }

            int relate = NetUtil.RelativeSequenceNumber(seq, _remoteWindowStart);
            int relateSeq = NetUtil.RelativeSequenceNumber(seq, _remoteSequence);

            if (relateSeq > WindowSize)
            {

#if RELIABLE_LOG
                Console.WriteLine($"rudp bad relative sequence {relateSeq}");
#endif
                return false;
            }

            if (relate < 0)
            {
                // 너무 오래된 패킷임
#if RELIABLE_LOG
                Console.WriteLine($"rudp ReliableInOrder too old");
#endif
                return false;
            }
            if (relate >= WindowSize * 2)
            {
                // 너무 새로운 패킷임
#if RELIABLE_LOG
                Console.WriteLine($"rudp ReliableInOrder too new");
#endif
                return false;
            }

            int ackIdx;
            int ackByte;
            int ackBit;

            // 응답패킷 데이터를 태우자
            lock (_ackPacket)
            {
                if (relate >= WindowSize)
                {
                    int newWindowStart = (_remoteWindowStart + relate - WindowSize + 1) % NetPacket.MaxSequence;
                    _ackPacket.Sequence = (ushort)newWindowStart;

                    // 예전 데이터를 정리
                    while (_remoteWindowStart != newWindowStart)
                    {
                        ackIdx = _remoteWindowStart % WindowSize;
                        ackByte = NetPacket.UserDataHeaderSize + ackIdx / BitsInByte;
                        ackBit = ackIdx % BitsInByte;

                        _ackPacket.RawData[ackByte] &= (byte)~(1 << ackBit);
                        _remoteWindowStart = (_remoteWindowStart + 1) % NetPacket.MaxSequence;
                    }
                }

                _sendAcks = true;

                ackIdx = seq % WindowSize;
                ackByte = NetPacket.UserDataHeaderSize + ackIdx / BitsInByte;
                ackBit = ackIdx % BitsInByte;
                if ((_ackPacket.RawData[ackByte] & (1 << ackBit)) != 0)
                {
#if RELIABLE_LOG
                    Console.WriteLine($"rudp ReliableInOrder duplicate");
#endif
                    NetPool.PacketPool.Free(poolingPacket);
                    return false;
                }

                _ackPacket.RawData[ackByte] |= (byte)(1 << ackBit);
            }

            if (seq == _remoteSequence)
            {
                _udpChannel.PacketReceived?.Invoke(_udpChannel, poolingPacket);

                _remoteSequence = (_remoteSequence + 1) % NetPacket.MaxSequence;

                if (_ordered)
                {
                    NetPacket p;
                    while ((p = _receivedPackets[_remoteSequence % WindowSize]) != null)
                    {
                        _receivedPackets[_remoteSequence % WindowSize] = null;
                        _udpChannel.PacketReceived?.Invoke(_udpChannel, p);
                        _remoteSequence = (_remoteSequence + 1) % NetPacket.MaxSequence;
                    }
                }
                else
                {
                    while (_earlyReceived[_remoteSequence % WindowSize])
                    {
                        _earlyReceived[_remoteSequence % WindowSize] = false;
                        _remoteSequence = (_remoteSequence + 1) % NetPacket.MaxSequence;
                    }
                }

                return true;
            }

            if (_ordered)
            {
                _receivedPackets[ackIdx] = poolingPacket;
            }
            else
            {
                _earlyReceived[ackIdx] = true;
                _udpChannel.PacketReceived?.Invoke(_udpChannel, poolingPacket);
            }

            return true;
        }

        private void ProcessAck(NetPacket packet)
        {
            if (packet.Size != _ackPacket.Size)
            {
#if RELIABLE_LOG
                Console.WriteLine("rudp Invalid acks packet size");
#endif
                return;
            }

            byte[] acksData = packet.RawData;
            lock (_pendingPackets)
            {
                ushort ackWindowStart = packet.Sequence;
                int windowRel = NetUtil.RelativeSequenceNumber(_localWindowStart, ackWindowStart);
                if (ackWindowStart >= NetPacket.MaxSequence || windowRel < 0)
                {
#if RELIABLE_LOG
                    Console.WriteLine($"rudp Bad window start {ackWindowStart} {windowRel}");
#endif
                    return;
                }

                if (windowRel >= WindowSize)
                {
#if RELIABLE_LOG
                    Console.WriteLine("rudp Old acks");
#endif
                    return;
                }

                for (int pendingSeq = _localWindowStart;
                    pendingSeq != _localSequence;
                    pendingSeq = (pendingSeq + 1) % NetPacket.MaxSequence)
                {
                    int rel = NetUtil.RelativeSequenceNumber(pendingSeq, ackWindowStart);
                    if (rel >= WindowSize)
                    {
#if RELIABLE_LOG
                        Console.WriteLine($"[PA]REL: {rel} {pendingSeq} {_localSequence} {ackWindowStart}");
#endif
                        break;
                    }

                    int pendingIdx = pendingSeq % WindowSize;
                    int currentByte = NetPacket.UserDataHeaderSize + pendingIdx / BitsInByte;
                    int currentBit = pendingIdx % BitsInByte;
                    if ((acksData[currentByte] & (1 << currentBit)) == 0)
                    {
                        Interlocked.Increment(ref _statistic.UdpPacketLossCount);
#if RELIABLE_LOG
                        Console.WriteLine($"[PA]False ack: {pendingSeq}");
#endif
                        continue;
                    }
                    if (pendingSeq == _localWindowStart)
                    {
                        _localWindowStart = (_localWindowStart + 1) % NetPacket.MaxSequence;
                    }
#if RELIABLE_LOG
                    Console.WriteLine($"ProcessAck End Id : {pendingIdx}");
#endif
                    _pendingPackets[pendingIdx].Clear();
                }
            }
        }
    }
}
