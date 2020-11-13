using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EuNet.Core
{
    public class UdpChannel : ChannelBase
    {
        public enum SendMode
        {
            // 즉시 보내기 (릴레이 지원 안됨)
            Immediately,

            // 버퍼를 통해서 보내기 (30ms 정도 지연이 생김) (릴레이는 버퍼만 지원됨)
            Buffered,
        }

        private UdpSocket _socket;
        private ChannelBase[] _udpChannels;

        public IPEndPoint LocalEndPoint;
        public IPEndPoint RemoteEndPoint;
        public IPEndPoint TempEndPoint;

        // 릴레이할 주소
        public IPEndPoint RelayEndPoint;

        // 최종적으로 홀펀칭된 원격주소 (홀펀칭중에는 UDP로 내가 받은 상대방주소)
        private IPEndPoint _punchedEndPoint;
        public IPEndPoint PunchedEndPoint => _punchedEndPoint;

        private int _mtu;
        public int Mtu => _mtu;

        // 다시 전송하는 시간 (ms)
        private long _resendDelay;
        public long ResendDelay => _resendDelay;

        private byte[] _sendBuffer;
        private int _sendBufferOffset;
        public volatile bool IsRunMtu;
        private volatile int _mtuId;
        private int _mtuElapsedTime;
        private int _mtuRemainCheckCount;
        private volatile bool _finishMtu;
        private CancellationTokenSource _cts;

        private PacketFragments _fragments;

        private NetPacket _pingPacket;
        private NetPacket _pongPacket;
        private Stopwatch _pingTimer;
        public volatile bool IsRunPing;
        private int _pingElapsedTime;
        private int _ping;
        public int Ping => _ping;
        private int _rtt;
        private int _rttCount;
        private int _avgRtt;

        // 원격지의 현재과 로컬의 현지시간과의 tick차이
        private long _remoteTickDelta;
        public DateTime RemoteUtcTime
        {
            get { return new DateTime(DateTime.UtcNow.Ticks + _remoteTickDelta); }
        }

        // 릴레이되어야 할 세션아이디 (서버의 경우 0이어야 함)
        private ushort _relaySessionId;

        public UdpChannel(IChannelOption channelOption, ILogger logger, NetStatistic statistic, ushort relaySessionId)
            : base(channelOption, logger, statistic)
        {
            _relaySessionId = relaySessionId;
            _sendBuffer = new byte[NetPacket.MaxUdpPacketSize];

            _fragments = new PacketFragments();

            _pingPacket = new NetPacket(PacketProperty.Ping, 0);
            _pingPacket.DeliveryMethod = DeliveryMethod.Unreliable;
            _pingPacket.Sequence = 1;

            _pongPacket = new NetPacket(PacketProperty.Pong, 0);
            _pongPacket.DeliveryMethod = DeliveryMethod.Unreliable;

            _pingTimer = new Stopwatch();

            _udpChannels = new ChannelBase[(int)DeliveryMethod.Max];

            _udpChannels[(int)DeliveryMethod.Unreliable] = new UnreliableChannel(channelOption, logger, statistic, this);
            _udpChannels[(int)DeliveryMethod.ReliableOrdered] = new ReliableChannel(channelOption, logger, statistic, this, true);
            _udpChannels[(int)DeliveryMethod.ReliableUnordered] = new ReliableChannel(channelOption, logger, statistic, this, false);
            _udpChannels[(int)DeliveryMethod.ReliableSequenced] = new SequencedChannel(channelOption, logger, statistic, this, true);
            _udpChannels[(int)DeliveryMethod.Sequenced] = new SequencedChannel(channelOption, logger, statistic, this, false);
        }

        public override void Init(CancellationTokenSource cts)
        {
            base.Init(cts);

            _sendBufferOffset = 0;
            _mtu = NetPacket.PossibleMtu[0];
            _punchedEndPoint = null;
            IsRunMtu = true;
            _mtuId = 0;
            _mtuRemainCheckCount = NetPacket.PossibleMtu.Length * 2;
            _mtuElapsedTime = 0;
            _finishMtu = false;
            _cts = new CancellationTokenSource();
            _fragments.Init();
            _resendDelay = 200;

            IsRunPing = true;
            _pingTimer.Reset();
            _pingElapsedTime = 0;
            _ping = 0;
            _rtt = 0;
            _rttCount = 0;
            _avgRtt = 0;
            _remoteTickDelta = 0;

            LocalEndPoint = null;
            RemoteEndPoint = null;
            TempEndPoint = null;
            RelayEndPoint = null;
            _punchedEndPoint = null;

            foreach (var channel in _udpChannels)
                channel?.Init(cts);
        }

        public override void Close()
        {
            base.Close();

            _cts.Cancel();
            _fragments.Clear();
            _pingTimer.Stop();

            foreach (var channel in _udpChannels)
                channel?.Close();
        }

        internal override void OnClosed()
        {
            base.OnClosed();

            foreach (var channel in _udpChannels)
                channel?.OnClosed();
        }

        internal void SetSocket(UdpSocket socket)
        {
            _socket = socket;
        }

        internal bool SetPunchedEndPoint(IPEndPoint endPoint, bool isForce = false)
        {
            if (isForce == false && _punchedEndPoint != null)
                return false;

            _punchedEndPoint = endPoint;
            return true;
        }

        public override bool Update(int elapsedTime)
        {
            if (IsRunPing)
            {
                // 핑 계산
                _pingElapsedTime += elapsedTime;
                if (_pingElapsedTime >= _channelOption.PingInterval)
                {
                    _pingElapsedTime = 0;

                    if (_pingTimer.IsRunning)
                        UpdateRoundTripTime((int)_pingTimer.ElapsedMilliseconds);

                    _pingTimer.Reset();
                    _pingTimer.Start();

                    _pingPacket.Sequence++;
                    SendTo(_pingPacket.RawData, 0, _pingPacket.Size, SendMode.Immediately);
                }
            }

            if(IsRunMtu)
            {
                // MTU Check
                _mtuElapsedTime += elapsedTime;
                if (_finishMtu == false &&
                    _mtuRemainCheckCount > 0 &&
                    _mtuElapsedTime >= _channelOption.MtuInterval)
                {
                    _mtuElapsedTime = 0;

                    int requestMtuId = _mtuId + 1;
                    if (requestMtuId >= NetPacket.PossibleMtu.Length)
                        _finishMtu = true;
                    else
                    {
                        int mtuSize = NetPacket.PossibleMtu[requestMtuId];
                        NetPacket packet = NetPool.PacketPool.Alloc(mtuSize);

                        try
                        {
                            packet.Property = PacketProperty.MtuCheck;
                            packet.DeliveryMethod = DeliveryMethod.Unreliable;
                            packet.RawData[3] = (byte)requestMtuId;

                            SendTo(packet.RawData, 0, packet.Size, SendMode.Immediately);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Exception happened in MtuCheckLoopAsync");
                        }
                        finally
                        {
                            NetPool.PacketPool.Free(packet);
                        }

                        _mtuRemainCheckCount--;
                    }
                }
            }

            // 신뢰성있는 패킷 처리를 하자
            if (GetReliableChannel(true)?.Update(elapsedTime) == false)
                return false;

            if (GetReliableChannel(false)?.Update(elapsedTime) == false)
                return false;

            if (GetSequencedChannel(true)?.Update(elapsedTime) == false)
                return false;

            if (GetSequencedChannel(false)?.Update(elapsedTime) == false)
                return false;

            // 버퍼된 데이터를 보내자
            SendBufferedData();

            return true;
        }

        internal void OnReceivedRawUdpData(byte[] data, int size, NetPacket cachedPacket, SocketError error, IPEndPoint endPoint)
        {
            //Console.WriteLine($"buffer size : {size}   packet size : {BitConverter.ToUInt16(data, 0)}");

            if (size < NetPacket.HeaderSize)
                return;

            try
            {
                PacketProperty property = cachedPacket.Property;
                DeliveryMethod deliveryMethod = cachedPacket.DeliveryMethod;

                switch (property)
                {

                    case PacketProperty.MtuCheck:
                        {
                            NetPacket packet = NetPool.PacketPool.Alloc(NetPacket.GetHeaderSize(PacketProperty.MtuOk));

                            try
                            {
                                byte getMtuId = data[3];

                                packet.Property = PacketProperty.MtuOk;
                                packet.DeliveryMethod = DeliveryMethod.Unreliable;
                                packet.RawData[3] = getMtuId;

                                SendTo(packet.RawData, 0, packet.Size, SendMode.Immediately);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Exception happened in MtuCheck");
                            }
                            finally
                            {
                                NetPool.PacketPool.Free(packet);
                            }
                        }
                        break;
                    case PacketProperty.MtuOk:
                        {
                            try
                            {
                                byte getMtuId = data[3];

                                if (getMtuId >= _mtuId)
                                {
                                    _mtuId = getMtuId;
                                    _mtu = NetPacket.PossibleMtu[getMtuId];

                                    if (_mtuId >= NetPacket.PossibleMtu.Length)
                                        _finishMtu = true;

                                    //Console.WriteLine($"SetMtu : {_mtu}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Exception happened in MtuOk");
                            }
                        }
                        break;
                    case PacketProperty.Ping:
                        {
                            ushort sequence = BitConverter.ToUInt16(data, 3);

                            if (NetUtil.RelativeSequenceNumber(sequence, _pongPacket.Sequence) > 0)
                            {
                                FastBitConverter.GetBytes(_pongPacket.RawData, 5, DateTime.UtcNow.Ticks);
                                _pongPacket.Sequence = sequence;
                                SendTo(_pongPacket.RawData, 0, _pongPacket.Size, SendMode.Immediately);
                            }
                        }
                        break;
                    case PacketProperty.Pong:
                        {
                            ushort sequence = BitConverter.ToUInt16(data, 3);

                            if (sequence == _pingPacket.Sequence)
                            {
                                _pingTimer.Stop();
                                int elapsedMs = (int)_pingTimer.ElapsedMilliseconds;
                                _remoteTickDelta = BitConverter.ToInt64(data, 5) + (elapsedMs * TimeSpan.TicksPerMillisecond) / 2 - DateTime.UtcNow.Ticks;
                                UpdateRoundTripTime(elapsedMs);

                                //Console.WriteLine($"Pong sequence : {sequence}  {elapsedMs} ms  {_remoteTickDelta} microseconds");
                            }
                        }
                        break;
                    default:
                        {
                            ReadPacket(data, size, _channelOption.PacketFilter);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception happened in OnReceivedRawUdpData");
                Close();
            }
        }

        protected override void OnPacketReceived(NetPacket poolingPacket)
        {
            // 파편화된 패킷을 처리하자
            if (poolingPacket.IsFragmented)
            {
                NetPacket resultPacket = _fragments.AddFragment(poolingPacket);
                if (resultPacket != null)
                    TossDeliveryMehtod(resultPacket);
            }
            else
            {
                TossDeliveryMehtod(poolingPacket);
            }
        }

        private void TossDeliveryMehtod(NetPacket poolingPacket)
        {
            switch (poolingPacket.DeliveryMethod)
            {
                case DeliveryMethod.Unreliable:
                    base.OnPacketReceived(poolingPacket);
                    break;
                case DeliveryMethod.ReliableOrdered:
                    GetReliableChannel(true)?.ProcessPacket(poolingPacket);
                    break;
                case DeliveryMethod.ReliableUnordered:
                    GetReliableChannel(false)?.ProcessPacket(poolingPacket);
                    break;
                case DeliveryMethod.ReliableSequenced:
                    GetSequencedChannel(true)?.ProcessPacket(poolingPacket);
                    break;
                case DeliveryMethod.Sequenced:
                    GetSequencedChannel(false)?.ProcessPacket(poolingPacket);
                    break;
                default:
                    NetPool.PacketPool.Free(poolingPacket);
                    break;
            }
        }


        private void UpdateRoundTripTime(int roundTripTime)
        {
            _ping = roundTripTime;

            _rtt += roundTripTime;
            _rttCount++;
            _avgRtt = _rtt / _rttCount;
            _resendDelay = 25 + (long)(_avgRtt * 2.1);
        }

        private ChannelBase GetChannel(DeliveryMethod method)
        {
            int index = (int)method;
            if (index >= _udpChannels.Length)
                return null;

            return _udpChannels[index];
        }

        private ReliableChannel GetReliableChannel(bool order)
        {
            if (order)
                return _udpChannels[(int)DeliveryMethod.ReliableOrdered] as ReliableChannel;
            return _udpChannels[(int)DeliveryMethod.ReliableUnordered] as ReliableChannel;
        }

        private SequencedChannel GetSequencedChannel(bool reliable)
        {
            if (reliable)
                return _udpChannels[(int)DeliveryMethod.ReliableSequenced] as SequencedChannel;
            return _udpChannels[(int)DeliveryMethod.Sequenced] as SequencedChannel;
        }

        public override void SendAsync(NetPacket poolingPacket)
        {
            var channel = GetChannel(poolingPacket.DeliveryMethod);
            if (channel == null)
            {
                NetPool.PacketPool.Free(poolingPacket);
                return;
            }

            int mtu = Mtu;
            if (poolingPacket.Size > mtu)
            {
                // 보낼수 있는 한계보다 큰 패킷이므로 쪼개서 보내자
                Interlocked.Increment(ref _statistic.UdpFragmentCount);

                try
                {
                    ushort currentFramentId = _fragments.GenerateId();

                    int headerSize = NetPacket.GetHeaderSize(poolingPacket.Property);
                    int dataSize = poolingPacket.Size - headerSize;

                    int maximumSize = mtu - headerSize;
                    int maximumDataSize = maximumSize - NetPacket.FragmentHeaderSize;
                    int totalPackets = dataSize / maximumDataSize + (dataSize % maximumDataSize == 0 ? 0 : 1);

                    for (ushort partIdx = 0; partIdx < totalPackets; partIdx++)
                    {
                        int sendLength = dataSize > maximumDataSize ? maximumDataSize : dataSize;

                        NetPacket p = NetPool.PacketPool.Alloc(sendLength + NetPacket.FragmentedHeaderTotalSize);
                        p.Property = poolingPacket.Property;
                        p.DeliveryMethod = poolingPacket.DeliveryMethod;
                        p.P2pSessionId = poolingPacket.P2pSessionId;
                        p.FragmentId = currentFramentId;
                        p.FragmentPart = partIdx;
                        p.FragmentsTotal = (ushort)totalPackets;
                        p.MarkFragmented();

                        Buffer.BlockCopy(poolingPacket.RawData, headerSize + partIdx * maximumDataSize, p.RawData, NetPacket.FragmentedHeaderTotalSize, sendLength);

                        IPacketFilter f = _channelOption.PacketFilter;
                        while (f != null)
                        {
                            p = f.Encode(p);
                            f = f.NextFilter;
                        }

                        channel.SendAsync(p);

                        dataSize -= sendLength;
                    }
                }
                finally
                {
                    NetPool.PacketPool.Free(poolingPacket);
                }

                return;
            }

            IPacketFilter filter = _channelOption.PacketFilter;
            while (filter != null)
            {
                poolingPacket = filter.Encode(poolingPacket);
                filter = filter.NextFilter;
            }

            Interlocked.Increment(ref _statistic.UdpPacketSentCount);

            channel.SendAsync(poolingPacket);
        }

        internal int SendTo(byte[] data, int offset, int size, SendMode sendMode)
        {
            if (sendMode == SendMode.Immediately)
            {
                IPEndPoint ep = _punchedEndPoint;

                if (ep == null)
                    return -1;

                Interlocked.Increment(ref _statistic.UdpSentCount);
                Interlocked.Add(ref _statistic.UdpSentBytes, size);

                SocketError error = SocketError.Success;
                return _socket.SendTo(data, offset, size, ep, ref error);
            }
            else
            {
                lock (_sendBuffer)
                {
                    if (_sendBufferOffset + size > _sendBuffer.Length ||
                        _sendBufferOffset + size > _mtu)
                    {
                        // 버퍼가 가득찼으므로 먼저 보내서 비우자
                        SendBufferedData();
                    }

                    if (_sendBufferOffset > 0)
                        Interlocked.Increment(ref _statistic.UdpSaveSentCount);

                    Buffer.BlockCopy(data, offset, _sendBuffer, _sendBufferOffset, size);
                    _sendBufferOffset += size;
                }
            }

            return -1;
        }

        internal void SendBufferedData()
        {
            bool isRelay = false;
            IPEndPoint ep = _punchedEndPoint;

            if (ep == null)
            {
                ep = RelayEndPoint;
                isRelay = true;
            }

            if (ep == null)
                return;

            // lock 에는 비용이 많이 드니 먼저 체크를 한번 하자
            if (_sendBufferOffset <= 0)
                return;

            lock (_sendBuffer)
            {
                // lock 직전에 바뀔 수 있으므로 다시 체크하자
                if (_sendBufferOffset > 0)
                {
                    Interlocked.Increment(ref _statistic.UdpSentCount);
                    Interlocked.Add(ref _statistic.UdpSentBytes, _sendBufferOffset);

                    if(isRelay)
                    {
                        Interlocked.Increment(ref _statistic.RelaySendCount);
                        Interlocked.Add(ref _statistic.RelaySendBytes, _sendBufferOffset);
                    }

                    SocketError error = SocketError.Success;

                    // P2p 또는 릴레이 세션아이디를 주입시킴. 여러개의 패킷이 겹쳐있어도 첫번째것만 체크하므로 이렇게 하면 됨
                    NetPacket.SetP2pSessionId(_sendBuffer, _relaySessionId);

                    _socket.SendTo(_sendBuffer, 0, _sendBufferOffset, ep, ref error);
                    _sendBufferOffset = 0;
                }
            }
        }
    }
}
