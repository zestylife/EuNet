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

        public NetViewRequestWaiter(NetView view)
        {
            View = view;
        }

        void IRequestWaiter.SendRequest(ISession session, NetDataWriter writer, DeliveryMethod deliveryMethod, DeliveryTarget deliveryTarget, int extra)
        {
            NetClient client = session as NetClient;
            if (client == null)
                throw new Exception($"Session must be NetClient instance");
            
            switch(deliveryTarget)
            {
                case DeliveryTarget.All:
                    {
                        // 다른사람들 보냄
                        client.P2pGroup.ViewNotification(writer.Data, 0, writer.Length, View.ViewId, deliveryMethod);
                        // 나를 호출함
                    }
                    break;
                case DeliveryTarget.Others:
                    {
                        // 다른사람들 보냄
                        client.P2pGroup.ViewNotification(writer.Data, 0, writer.Length, View.ViewId, deliveryMethod);
                    }
                    break;
                case DeliveryTarget.Master:
                    {
                        var target = client.P2pGroup.GetMasterMember();
                        if (target == null)
                            return;

                        target.ViewNotification(writer.Data, 0, writer.Length, View.ViewId, deliveryMethod);
                    }
                    break;
                case DeliveryTarget.Target:
                    {
                        var target = client.P2pGroup.Find((ushort)extra);
                        if (target == null)
                            return;

                        target.ViewNotification(writer.Data, 0, writer.Length, View.ViewId, deliveryMethod);
                    }
                    break;
            }
        }

        Task IRequestWaiter.SendRequestAndWait(ISession session, NetDataWriter writer, TimeSpan? timeout, DeliveryMethod deliveryMethod, int extra)
        {
            if (deliveryMethod != DeliveryMethod.ReliableOrdered &&
                deliveryMethod != DeliveryMethod.ReliableUnordered)
                throw new Exception($"Not support {deliveryMethod} in SendRequestAndWait");

            NetClient client = session as NetClient;
            if (client == null)
                throw new Exception($"Session must be NetClient instance");

            var target = client.P2pGroup.Find((ushort)extra);
            var task = target.ViewRequestAsync(writer.Data, 0, writer.Length, View.ViewId, deliveryMethod, timeout);

            try
            {
                return task;
            }
            catch (Exception e)
            {
                throw e.InnerException ?? e;
            }
        }

        Task<NetDataBufferReader> IRequestWaiter.SendRequestAndReceive(ISession session, NetDataWriter writer, TimeSpan? timeout, DeliveryMethod deliveryMethod, int extra)
        {
            if (deliveryMethod != DeliveryMethod.ReliableOrdered &&
                deliveryMethod != DeliveryMethod.ReliableUnordered)
                throw new Exception($"Not support {deliveryMethod} in SendRequestAndWait");

            NetClient client = session as NetClient;
            if (client == null)
                throw new Exception($"Session must be NetClient instance");

            var target = client.P2pGroup.Find((ushort)extra);
            var task = target.ViewRequestAsync(writer.Data, 0, writer.Length, View.ViewId, deliveryMethod, timeout);

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
