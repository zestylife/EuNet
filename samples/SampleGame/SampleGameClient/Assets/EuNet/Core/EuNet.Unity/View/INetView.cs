using EuNet.Core;

namespace EuNet.Unity
{
    public interface INetView
    {
        int ViewId { get; }

        void OnNetInstantiate(NetDataReader reader);
        void OnNetDestroy(NetDataReader reader);
        void SendMessage(NetDataWriter writer, DeliveryTarget deliveryTarget, DeliveryMethod deliveryMethod);
        void OnMessage(NetDataReader reader);
        void OnNetSync(NetProtocol procorol, NetDataReader reader);
    }
}
