using EuNet.Core;
using System;
using System.Threading.Tasks;

namespace SampleGameServer
{
    public partial class UserSession : ServerSession
    {
        public UserSession(ushort sessionId, TcpChannel tcpChannel, UdpChannel udpChannel)
            : base(sessionId, tcpChannel, udpChannel)
        {

        }

        protected override void OnConnected()
        {
            base.OnConnected();
        }

        protected override Task OnClosed()
        {
            return base.OnClosed();
        }

        public override void OnError(Exception ex)
        {
            base.OnError(ex);

            Console.WriteLine(ex.ToString());
        }

        public override Task OnReceive(NetDataReader reader)
        {
            var text = reader.ReadString();
            Console.WriteLine($"received : {text}");

            switch (text)
            {
                case "join":
                    {
                        var result = Program.Instance.P2pGroup.Join(this);

                        var writer = NetPool.DataWriterPool.Alloc();
                        try
                        {
                            writer.Write("join");
                            writer.Write(result);

                            SendAsync(writer, DeliveryMethod.Tcp);
                        }
                        finally
                        {
                            NetPool.DataWriterPool.Free(writer);
                        }
                    }
                    break;
                case "leave":
                    {
                        Program.Instance.P2pGroup.Leave(this);
                    }
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
