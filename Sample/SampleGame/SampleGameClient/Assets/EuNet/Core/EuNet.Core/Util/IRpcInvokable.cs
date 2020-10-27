using System.Threading.Tasks;

namespace EuNet.Core
{
    public interface IRpcInvokable
    {
        Task<bool> Invoke(object target, NetDataReader reader, NetDataWriter writer);
    }
}
