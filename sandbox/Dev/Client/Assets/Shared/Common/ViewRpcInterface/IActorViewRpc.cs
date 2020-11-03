using EuNet.Rpc;
using System.Threading.Tasks;
using UnityEngine;

namespace Common
{
    public interface IActorViewRpc : IViewRpc
    {
        Task OnSetMoveDirection(float x, float y);
        Task<Color> OnTest(Vector3 position, Quaternion rotation);
    }
}
