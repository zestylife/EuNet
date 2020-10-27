using System;
using System.Threading;

namespace EuNet.Core
{
    public interface IChannel
    {
        bool Update(int elapsedTime);

        void SendAsync(NetPacket poolingPacket);
        Action<IChannel, NetPacket> PacketReceived { get; set; }

        void Init(CancellationTokenSource cts);
        void Close();
    }
}
