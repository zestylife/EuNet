using EuNet.Core;

namespace EuNet.Unity
{
    public interface INetViewPeriodicSync
    {
        bool OnViewPeriodicSyncSerialize(NetDataWriter writer);
        void OnViewPeriodicSyncDeserialize(NetDataReader reader);
    }
}
