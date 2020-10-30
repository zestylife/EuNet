using EuNet.Core;
using System;
using System.Threading.Tasks;

namespace EuNet.Rpc
{
    public abstract class RpcRequester
    {
        public ISession Target { get; protected internal set; }
        public TimeSpan? Timeout { get; protected internal set; }
        public IRequestWaiter RequestWaiter { get; protected internal set; }
        public DeliveryMethod DeliveryMethod { get; protected internal set; }
        public DeliveryTarget DeliveryTarget { get; protected internal set; }
        public int Extra { get; protected internal set; }

        abstract public Type InterfaceType { get; }

        protected RpcRequester(ISession target)
        {
            Target = target;
            RequestWaiter = DefaultRequestWaiter.Instance;
        }

        protected RpcRequester(ISession target, IRequestWaiter requestWaiter, TimeSpan? timeout)
        {
            Target = target;
            RequestWaiter = requestWaiter ?? DefaultRequestWaiter.Instance;
            Timeout = timeout;
        }

        protected void SendRequest(
            NetDataWriter writer)
        {
            RequestWaiter.SendRequest(Target, writer, DeliveryMethod, DeliveryTarget, Extra);
        }

        protected Task SendRequestAndWait(NetDataWriter writer)
        {
            return RequestWaiter.SendRequestAndWait(Target, writer, Timeout, DeliveryMethod, Extra);
        }

        protected Task<NetDataBufferReader> SendRequestAndReceive(NetDataWriter writer)
        {
            return RequestWaiter.SendRequestAndReceive(Target, writer, Timeout, DeliveryMethod, Extra);
        }
    }
}
