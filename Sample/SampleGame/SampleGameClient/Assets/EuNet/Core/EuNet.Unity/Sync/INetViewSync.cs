using EuNet.Core;

namespace EuNet.Unity
{
    public interface INetViewSync
    {
        NetView View { get; }

        void Update(float elapsedTime, bool isMine);
        void OnReceiveSync(NetDataReader reader);
    }
}
