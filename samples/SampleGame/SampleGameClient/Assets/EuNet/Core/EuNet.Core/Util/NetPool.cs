using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EuNet.Server"), InternalsVisibleTo("EuNet.Client"), InternalsVisibleTo("EuNet.Core.Tests"), InternalsVisibleTo("EuNet.Server.Tests"), InternalsVisibleTo("EuNet.Benchmark")]
namespace EuNet.Core
{
    /// <summary>
    /// 네트워크 풀을 모아놓은 클래스
    /// </summary>
    public static class NetPool
    {
        /// <summary>
        /// DataWriter Pool
        /// </summary>
        public static NetDataWriterPool DataWriterPool = new NetDataWriterPool(1000);

        /// <summary>
        /// NetPacket Pool
        /// </summary>
        internal static NetPacketPool PacketPool = new NetPacketPool(10000);

        /// <summary>
        /// Buffer Pool
        /// </summary>
        public static BufferPool BufferPool = new BufferPool(1000);
    }
}
