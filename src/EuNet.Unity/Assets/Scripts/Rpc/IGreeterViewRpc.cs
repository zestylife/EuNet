using System.Threading.Tasks;
using UnityEngine;

namespace EuNet.Rpc.Test.Interface
{
    public interface IGreeterViewRpc : IViewRpc
    {
        Task<bool> Greet(int value);
        Task<Color> UnityType(Vector3 position, Quaternion rotation);
    }
}
