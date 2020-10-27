using EuNet.Core;
using System.Threading.Tasks;

namespace EuNet.Rpc
{
    public interface IAsyncInvokable
    {
        Task InvokeAsync(object target, ISession session, NetDataReader reader, NetDataWriter writer);
    }
}
