using EuNet.Rpc;
using System.Threading.Tasks;
using UnityEngine;

namespace Common
{
    public interface IActorViewRpc : IViewRpc
    {
        Task OnSetMoveDirection(float moveX, float moveY, Vector3 position);
        Task<Color> OnTest(Vector3 position, Quaternion rotation);
    }
}
