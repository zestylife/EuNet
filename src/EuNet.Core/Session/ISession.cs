using System;
using System.Threading.Tasks;

namespace EuNet.Core
{
    public interface ISession
    {
        ushort SessionId { get; }
        TcpChannel TcpChannel { get; }
        UdpChannel UdpChannel { get; }
        SessionState State { get; }
        SessionRequest SessionRequest { get; }

        void Init(SessionInitializeInfo info);
        void Close();
        void Update(int elapsedTime);
        void SendAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod);
        void SendAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod);
        void SendRawAsync(NetPacket poolingPacket, DeliveryMethod deliveryMethod);
        Task<NetDataBufferReader> RequestAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod, TimeSpan? timeout);
        Task<NetDataBufferReader> RequestAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod, TimeSpan? timeout);
        Task OnReceive(NetDataReader dataReader);
        void OnError(Exception exception);
    }
}
