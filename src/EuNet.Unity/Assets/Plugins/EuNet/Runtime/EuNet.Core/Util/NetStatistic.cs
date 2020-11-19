using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EuNet.Core
{
    /// <summary>
    /// 네트워크 통계
    /// </summary>
    public sealed class NetStatistic
    {
        /// <summary>
        /// TCP 총 받은 바이트
        /// </summary>
        public long TcpReceivedBytes;

        /// <summary>
        /// UDP 총 받은 바이트
        /// </summary>
        public long UdpReceivedBytes;

        /// <summary>
        /// TCP 총 보낸 바이트
        /// </summary>
        public long TcpSentBytes;

        /// <summary>
        /// UDP 총 보낸 바이트
        /// </summary>
        public long UdpSentBytes;

        /// <summary>
        /// TCP 총 받은 개수
        /// </summary>
        public long TcpReceivedCount;

        /// <summary>
        /// UDP 총 받은 개수
        /// </summary>
        public long UdpReceivedCount;

        /// <summary>
        /// TCP 총 보낸 개수
        /// </summary>
        public long TcpSentCount;

        /// <summary>
        /// UDP 총 보낸 개수
        /// </summary>
        public long UdpSentCount;

        /// <summary>
        /// TCP 총 보낸 Packet 개수
        /// </summary>
        public long TcpPacketSentCount;

        /// <summary>
        /// UDP 총 보낸 Packet 개수
        /// </summary>
        public long UdpPacketSentCount;

        /// <summary>
        /// 총 받은 Packet 개수
        /// </summary>
        public long PacketReceivedCount;

        /// <summary>
        /// UDP 다시 보낸 개수.
        /// RUDP 사용시 Ack를 시간안에 받지 못하면 재전송함.
        /// </summary>
        public long UdpResentCount;

        /// <summary>
        /// RUDP 보낸 Packet 개수
        /// </summary>
        public long UdpReliablePacketSentCount;

        /// <summary>
        /// UDP 소실된 개수
        /// </summary>
        public long UdpPacketLossCount;

        /// <summary>
        /// UDP 모아서 전송하여 아낀 전송 개수
        /// </summary>
        public long UdpSaveSentCount;

        /// <summary>
        /// UDP 데이터가 너무 커서 분할하여 보낸 패킷의 원본 개수
        /// </summary>
        public long UdpFragmentCount;

        /// <summary>
        /// UDP 릴레이로 데이터를 보낸 개수 (클라이언트)
        /// </summary>
        public long RelaySendCount;

        /// <summary>
        /// UDP 릴레이로 데이터를 보낸 바이트 (클라이언트)
        /// </summary>
        public long RelaySendBytes;

        /// <summary>
        /// UDP 릴레이를 처리한 개수 (서버)
        /// </summary>
        public long RelayServCount;

        /// <summary>
        /// UDP 릴레이를 처리한 바이트 (서버)
        /// </summary>
        public long RelayServBytes;

        /// <summary>
        /// RPC 통계를 수집할지 여부
        /// </summary>
        public volatile bool IsGatherRpc = false;

        /// <summary>
        /// 요청한 RPC 맵
        /// </summary>
        public Dictionary<int, int> _requestRpcMap = new Dictionary<int, int>();

        /// <summary>
        /// 요청받은 RPC 맵
        /// </summary>
        public Dictionary<int, int> _responseRpcMap = new Dictionary<int, int>();

        /// <summary>
        /// UDP 패킷 소실 퍼센트 ( 0.0 ~ 100.0 )
        /// </summary>
        public float UdpPacketLossPercent
        {
            get { return UdpReliablePacketSentCount == 0 ? 0.0f : (float)UdpPacketLossCount * 100.0f / (float)UdpReliablePacketSentCount; }
        }

        /// <summary>
        /// 모든 채널로 보낸 총 개수
        /// </summary>
        public long TotalSentCount { get { return TcpSentCount + UdpSentCount; } }

        /// <summary>
        /// 모든 채널로 받은 총 개수
        /// </summary>
        public long TotalReceivedCount { get { return TcpReceivedCount + UdpReceivedCount; } }

        /// <summary>
        /// 모든 채널로 보낸 총 바이트
        /// </summary>
        public long TotalSentBytes { get { return TcpSentBytes + UdpSentBytes; } }

        /// <summary>
        /// 모든 채널로 받은 총 바이트
        /// </summary>
        public long TotalReceivedBytes { get { return TcpReceivedBytes + UdpReceivedBytes; } }

        /// <summary>
        /// 모든 채널로 보낸 총 Packet 개수
        /// </summary>
        public long TotalPacketSentCount { get { return TcpPacketSentCount + UdpPacketSentCount; } }
        
        public void IncreaseRequestRpc(int requestId, int length)
        {
            if (IsGatherRpc == false)
                return;

            lock(_requestRpcMap)
            {
                if (_requestRpcMap.ContainsKey(requestId))
                    _requestRpcMap[requestId]++;
                else _requestRpcMap[requestId] = 0;
            }
        }

        public void IncreaseResponseRpc(int requestId)
        {
            if (IsGatherRpc == false)
                return;

            lock (_responseRpcMap)
            {
                if (_responseRpcMap.ContainsKey(requestId))
                    _responseRpcMap[requestId]++;
                else _responseRpcMap[requestId] = 0;
            }
        }

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
