using EuNet.Core;
using System;
using System.Threading.Tasks;

namespace EuNet.Rpc
{
    public interface IRequestWaiter
    {
        void SendRequest(ISession session, NetDataWriter writer, DeliveryMethod deliveryMethod, DeliveryTarget deliveryTarget, int extra);
        Task SendRequestAndWait(ISession session, NetDataWriter writer, TimeSpan? timeout, DeliveryMethod deliveryMethod, int extra);
        Task<NetDataBufferReader> SendRequestAndReceive(ISession session, NetDataWriter writer, TimeSpan? timeout, DeliveryMethod deliveryMethod, int extra);
    }
}
