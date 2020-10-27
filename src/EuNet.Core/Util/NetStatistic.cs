using System.Text;
using System.Threading;

namespace EuNet.Core
{
    public sealed class NetStatistic
    {
        public long TcpReceivedBytes;
        public long UdpReceivedBytes;

        public long TcpSentBytes;
        public long UdpSentBytes;

        // 받은 횟수 (커널기준)
        public long TcpReceivedCount;
        public long UdpReceivedCount;

        // 보낸 횟수 (커널기준)
        public long TcpSentCount;
        public long UdpSentCount;

        // 패킷 보낸 횟수
        public long TcpPacketSentCount;
        public long UdpPacketSentCount;

        // 받은 패킷 개수
        public long PacketReceivedCount;

        // 다시 보낸 수
        public long UdpResentCount;

        public long UdpReliablePacketSentCount;

        // 소실된 패킷 수
        public long UdpPacketLossCount;

        // 모아 보내서 아낀 패킷 수
        public long UdpSaveSentCount;

        // 쪼개서 보낸 패킷의 원본수
        public long UdpFragmentCount;

        // 클라이언트에서 릴레이로 보내는 수치
        public long RelaySendCount;
        public long RelaySendBytes;

        // 서버에서 릴레이를 받아서 처리하는 수치
        public long RelayServCount;
        public long RelayServBytes;

        public float UdpPacketLossPercent
        {
            get { return UdpReliablePacketSentCount == 0 ? 0.0f : (float)UdpPacketLossCount * 100.0f / (float)UdpReliablePacketSentCount; }
        }

        public long TotalSentCount { get { return TcpSentCount + UdpSentCount; } }
        public long TotalReceivedCount { get { return TcpReceivedCount + UdpReceivedCount; } }

        public long TotalSentBytes { get { return TcpSentBytes + UdpSentBytes; } }
        public long TotalReceivedBytes { get { return TcpReceivedBytes + UdpReceivedBytes; } }

        public long TotalPacketSentCount { get { return TcpPacketSentCount + UdpPacketSentCount; } }

        public void Reset()
        {
            Interlocked.Exchange(ref TcpReceivedBytes, 0);
            Interlocked.Exchange(ref UdpReceivedBytes, 0);
            Interlocked.Exchange(ref UdpSentBytes, 0);
            Interlocked.Exchange(ref TcpSentBytes, 0);
            Interlocked.Exchange(ref UdpSentBytes, 0);
            Interlocked.Exchange(ref TcpReceivedCount, 0);
            Interlocked.Exchange(ref UdpReceivedCount, 0);
            Interlocked.Exchange(ref TcpSentCount, 0);
            Interlocked.Exchange(ref UdpSentCount, 0);
            Interlocked.Exchange(ref TcpPacketSentCount, 0);
            Interlocked.Exchange(ref UdpPacketSentCount, 0);
            Interlocked.Exchange(ref PacketReceivedCount, 0);
            Interlocked.Exchange(ref UdpResentCount, 0);
            Interlocked.Exchange(ref UdpReliablePacketSentCount, 0);
            Interlocked.Exchange(ref UdpPacketLossCount, 0);
            Interlocked.Exchange(ref UdpSaveSentCount, 0);
            Interlocked.Exchange(ref UdpFragmentCount, 0);

            Interlocked.Exchange(ref RelaySendCount, 0);
            Interlocked.Exchange(ref RelaySendBytes, 0);
            Interlocked.Exchange(ref RelayServCount, 0);
            Interlocked.Exchange(ref RelayServBytes, 0);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"- NetStatistic -");
            builder.AppendLine($"TcpReceivedBytes : {TcpReceivedBytes}");
            builder.AppendLine($"UdpReceivedBytes : {UdpReceivedBytes}");
            builder.AppendLine($"TcpSentBytes : {TcpSentBytes}");
            builder.AppendLine($"UdpSentBytes : {UdpSentBytes}");
            builder.AppendLine($"TcpReceivedCount : {TcpReceivedCount}");
            builder.AppendLine($"UdpReceivedCount : {UdpReceivedCount}");
            builder.AppendLine($"TcpSentCount : {TcpSentCount}");
            builder.AppendLine($"UdpSentCount : {UdpSentCount}");
            builder.AppendLine($"TcpPacketSentCount : {TcpPacketSentCount}");
            builder.AppendLine($"UdpPacketSentCount : {UdpPacketSentCount}");
            builder.AppendLine($"PacketReceivedCount : {PacketReceivedCount}");
            builder.AppendLine($"UdpResentCount : {UdpResentCount}");
            builder.AppendLine($"UdpReliablePacketSentCount : {UdpReliablePacketSentCount}");
            builder.AppendLine($"UdpPacketLossCount : {UdpPacketLossCount}");
            builder.AppendLine($"UdpSaveSentCount : {UdpSaveSentCount}");
            builder.AppendLine($"UdpFragmentCount : {UdpFragmentCount}");
            builder.AppendLine($"RelaySendCount : {RelaySendCount}");
            builder.AppendLine($"RelaySendBytes : {RelaySendBytes}");
            builder.AppendLine($"RelayServCount : {RelayServCount}");
            builder.AppendLine($"RelayServBytes : {RelayServBytes}");

            builder.AppendLine($"UdpPacketLossPercent : {UdpPacketLossPercent}");
            builder.AppendLine($"TotalSentCount : {TotalSentCount}");
            builder.AppendLine($"TotalReceivedCount : {TotalReceivedCount}");
            builder.AppendLine($"TotalSentBytes : {TotalSentBytes}");
            builder.AppendLine($"TotalReceivedBytes : {TotalReceivedBytes}");
            builder.AppendLine($"TotalPacketSentCount : {TotalPacketSentCount}");

            return builder.ToString();
        }
    }
}
