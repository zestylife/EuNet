using EuNet.Core;
using System;
using System.Threading.Tasks;

namespace EuNet.Rpc
{
    public interface IRequestWaiter
    {
        void SendRequest(ISession session, NetDataWriter writer);
        Task SendRequestAndWait(ISession session, NetDataWriter writer, TimeSpan? timeout);
        Task<NetDataBufferReader> SendRequestAndReceive(ISession session, NetDataWriter writer, TimeSpan? timeout);
    }
}
