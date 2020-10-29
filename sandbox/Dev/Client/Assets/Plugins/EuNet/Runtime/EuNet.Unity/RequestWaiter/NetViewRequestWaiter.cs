using EuNet.Client;
using EuNet.Core;
using EuNet.Rpc;
using System;
using System.Threading.Tasks;

namespace EuNet.Unity
{
    public class NetViewRequestWaiter : IRequestWaiter
    {
        public readonly NetView View;
        public readonly DeliveryMethod DeliveryMethod;

        public NetViewRequestWaiter(NetView view, DeliveryMethod deliveryMethod)
        {
            View = view;
            DeliveryMethod = deliveryMethod;
        }

        void IRequestWaiter.SendRequest(ISession session, NetDataWriter writer)
        {
            NetClient client = session as NetClient;
            if (client != null)
                client.P2pGroup.ViewNotification(writer.Data, 0, writer.Length, View.ViewId, DeliveryMethod);
            else session.SessionRequest.ViewNotification(writer.Data, 0, writer.Length, View.ViewId, DeliveryMethod);
        }

        Task IRequestWaiter.SendRequestAndWait(ISession session, NetDataWriter writer, TimeSpan? timeout)
        {
            if (DeliveryMethod != DeliveryMethod.ReliableOrdered &&
                DeliveryMethod != DeliveryMethod.ReliableUnordered)
                throw new Exception($"Not support {DeliveryMethod} in SendRequestAndWait");

            // 그룹의 여러 인원에게 보내고 결과를 받을 수 없으므로 지원되지 않음
            if ((session is P2pSession) == false)
                throw new Exception("Not support P2pSession in SendRequestAndWait");

            var task = session.SessionRequest.ViewRequestAsync(writer.Data, 0, writer.Length, View.ViewId, DeliveryMethod, timeout);

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
            if (DeliveryMethod != DeliveryMethod.ReliableOrdered &&
                DeliveryMethod != DeliveryMethod.ReliableUnordered)
                throw new Exception($"Not support {DeliveryMethod} in SendRequestAndWait");

            // 그룹의 여러 인원에게 보내고 결과를 받을 수 없으므로 지원되지 않음
            if ((session is P2pSession) == false)
                throw new Exception("Not support P2pSession in SendRequestAndReceive");

            var task = session.SessionRequest.ViewRequestAsync(writer.Data, 0, writer.Length, View.ViewId, DeliveryMethod, timeout);

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
