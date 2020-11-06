using System;
using System.Threading;

namespace EuNet.Core
{
    public abstract class ChannelBase : IChannel
    {
        public abstract void SendAsync(NetPacket poolingPacket);
        public abstract bool Update(int elapsedTime);

        public Action<IChannel, NetPacket> PacketReceived { get; set; }

        protected readonly IChannelOption _channelOption;
        protected readonly ILogger _logger;
        protected readonly NetStatistic _statistic;

        private CancellationTokenSource _sessionCts;

        public ChannelBase(IChannelOption channelOption, ILogger logger, NetStatistic statistic)
        {
            _channelOption = channelOption;
            _logger = logger;
            _statistic = statistic;
        }

        public virtual void Init(CancellationTokenSource cts)
        {
            _sessionCts = cts;
        }

        public virtual void Close()
        {
            try
            {
                _sessionCts?.Cancel();
                _sessionCts = null;
            }
            catch
            {

            }
        }

        protected virtual void OnPacketReceived(NetPacket poolingPacket)
        {
            PacketReceived?.Invoke(this, poolingPacket);
        }

        // 버퍼를 입력하여 패킷을 읽어서 처리합니다. buffer는 내용이 수정될 수 있습니다.
        protected int ReadPacket(byte[] buffer, int totalReceivedSize, IPacketFilter packetFilter)
        {
            int offset = 0;
            int remainSize = totalReceivedSize;
            while (remainSize >= NetPacket.HeaderSize)
            {
                int packetSize = BitConverter.ToUInt16(buffer, offset);

                // 최대 패킷크기를 초과했으니 에러
                if (packetSize >= NetPacket.MaxTcpPacketSize)
                {
                    Close();
                    return -1;
                }

                // 받은 사이즈가 실제 패킷사이즈보다 작으니 기다리자
                if (remainSize < packetSize)
                    break;

                NetPacket packet = NetPool.PacketPool.Alloc(packetSize);
                Buffer.BlockCopy(buffer, offset, packet.RawData, 0, packetSize);

                // 패킷을 가공하자
                IPacketFilter filter = packetFilter;
                while (filter != null)
                {
                    packet = filter.Decode(packet);
                    filter = filter.NextFilter;
                }

                // 유저 패킷 처리
                OnPacketReceived(packet);

                offset += packetSize;
                remainSize -= packetSize;

                Interlocked.Increment(ref _statistic.PacketReceivedCount);
            }

            return offset;
        }

        internal virtual void OnClosed()
        {

        }
    }
}
