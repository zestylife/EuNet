using EuNet.Core;

namespace EuNet.Unity
{
    public interface INetViewHandler
    {
        void OnViewInstantiate(NetDataReader reader);
        void OnViewDestroy(NetDataReader reader);
        void OnViewMessage(NetDataReader reader);
    }
}
