using EuNet.Core;
using System.Net;

namespace EuNet.Server
{
    public class P2pMember : P2pMemberBase
    {
        public void SendJoinP2pGroup(ushort groupId, ushort sessionId, ushort masterSessionId, IPEndPoint remoteEndPoint, IPEndPoint localEndPoint)
        {
            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write(groupId);
                writer.Write(sessionId);
                writer.Write(masterSessionId);
                writer.Write(remoteEndPoint);
                writer.Write(localEndPoint);

                NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.JoinP2p, writer);
                packet.DeliveryMethod = DeliveryMethod.Tcp;

                (Session as ServerSession)?.SendRawAsync(packet, packet.DeliveryMethod);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }

        public void SendLeaveP2pGroup(ushort groupId, ushort sessionId, ushort masterSessionId)
        {
            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write(groupId);
                writer.Write(sessionId);
                writer.Write(masterSessionId);

                NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.LeaveP2p, writer);
                packet.DeliveryMethod = DeliveryMethod.Tcp;

                (Session as ServerSession)?.SendRawAsync(packet, packet.DeliveryMethod);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }

        public void SendChangeMasterP2pGroup(ushort groupId, ushort masterSessionId)
        {
            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write(groupId);
                writer.Write(masterSessionId);

                NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.ChangeP2pMaster, writer);
                packet.DeliveryMethod = DeliveryMethod.Tcp;

                (Session as ServerSession)?.SendRawAsync(packet, packet.DeliveryMethod);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }
    }
}
