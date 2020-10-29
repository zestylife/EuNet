using EuNet.Core;
using System;
using System.Threading.Tasks;

namespace EuNet.Rpc
{
    internal class DefaultRequestWaiter : IRequestWaiter
    {
        public static IRequestWaiter Instance = new DefaultRequestWaiter();

        void IRequestWaiter.SendRequest(ISession session, NetDataWriter writer)
        {
            session.SessionRequest.Notification(writer.Data, 0, writer.Length, DeliveryMethod.Tcp);
        }

        Task IRequestWaiter.SendRequestAndWait(ISession session, NetDataWriter writer, TimeSpan? timeout)
        {
            var task = session.SessionRequest.RequestAsync(writer.Data, 0, writer.Length, DeliveryMethod.Tcp, timeout);

            try
            {
                return task;
            }
            catch (Exception e)
            {
                throw e.InnerException ?? e;
            }
        }

        Task<NetDataBufferReader> IRequestWaiter.SendRequestAndReceive(ISession session, NetDataWriter writer, TimeSpan? timeout)
        {
            var task = session.SessionRequest.RequestAsync(writer.Data, 0, writer.Length, DeliveryMethod.Tcp, timeout);

            try
            {
                return task;
            }
            catch (Exception e)
            {
                throw e.InnerException ?? e;
            }
        }
    }
}
