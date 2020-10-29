using EuNet.Rpc;
using System.Threading.Tasks;

namespace Common
{
    public interface IActorViewRpc : IViewRpc
    {
        Task OnSetMoveDirection(float x, float y);
    }
}
