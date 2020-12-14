using EuNet.Client;
using EuNet.Core;
using EuNet.Rpc;
using System;
using System.Threading.Tasks;
using UnityEngine;

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

            if(client.State != SessionState.Connected)
            {
                Debug.LogWarning("[NetViewRequestWaiter.SendRequest] Not connected to server");
                return;
            }

            if (client.P2pGroup == null)
            {
                Debug.LogWarning("[NetViewRequestWaiter.SendRequest] Not joined p2p group");
                return;
            }

            switch (deliveryTarget)
            {
                case DeliveryTarget.All:
                    {
                        // 다른사람들 보냄
                        client.P2pGroup.ViewNotification(writer.Data, 0, writer.Length, View.ViewId, deliveryMethod);

                        // 나를 호출함
                        CallSelfNotification(client, writer, deliveryMethod);
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
                        if(client.P2pGroup.MasterIsMine())
                        {
                            // 자신이 마스터임
                            CallSelfNotification(client, writer, deliveryMethod);
                        }
                        else
                        {
                            // 다른유저가 마스터임
                            var target = client.P2pGroup.GetMasterMember();
                            if (target == null)
                                return;

                            target.ViewNotification(writer.Data, 0, writer.Length, View.ViewId, deliveryMethod);
                        }
                    }
                    break;
                case DeliveryTarget.Target:
                    {
                        ushort targetSessionId = (ushort)extra;
                        if (targetSessionId == 0)
                            throw new Exception("Target's SessionId must not be 0");

                        if (client.SessionId == targetSessionId)
                        {
                            // 본인이 타겟임
                            CallSelfNotification(client, writer, deliveryMethod);
                        }
                        else
                        {
                            var target = client.P2pGroup.Find(targetSessionId);
                            if (target == null)
                                return;

                            target.ViewNotification(writer.Data, 0, writer.Length, View.ViewId, deliveryMethod);
                        }
                    }
                    break;
            }
        }

        private void CallSelfNotification(NetClient client, NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            NetPool.DataWriterPool.Use(w =>
            {
                w.Write(View.ViewId);
                w.WriteOnlyData(writer.Data, 0, writer.Length);

                NetDataReader r = new NetDataReader(w);
                client.OnViewRequestReceive(client, r, new NetDataWriter());
            });
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
