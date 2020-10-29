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

        protected void SendRequest(NetDataWriter writer)
        {
            RequestWaiter.SendRequest(Target, writer);
        }

        protected Task SendRequestAndWait(NetDataWriter writer)
        {
            return RequestWaiter.SendRequestAndWait(Target, writer, Timeout);
        }

        protected Task<NetDataBufferReader> SendRequestAndReceive(NetDataWriter writer)
        {
            return RequestWaiter.SendRequestAndReceive(Target, writer, Timeout);
        }
    }
}
