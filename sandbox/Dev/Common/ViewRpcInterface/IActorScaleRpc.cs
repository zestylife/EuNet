using EuNet.Rpc;
using System.Threading.Tasks;
using UnityEngine;

namespace Common
{
    public interface IActorScaleRpc : IViewRpc
    {
        Task OnSetScale(Vector3 scale);
    }
}
