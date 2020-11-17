using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EuNet.Server"), InternalsVisibleTo("EuNet.Client"), InternalsVisibleTo("EuNet.Core.Tests"), InternalsVisibleTo("EuNet.Server.Tests"), InternalsVisibleTo("EuNet.Benchmark")]
namespace EuNet.Core
{
    public static class NetPool
    {
        public static NetDataWriterPool DataWriterPool = new NetDataWriterPool(1000);
        internal static NetPacketPool PacketPool = new NetPacketPool(10000);
        public static BufferPool BufferPool = new BufferPool(1000);
    }
}
