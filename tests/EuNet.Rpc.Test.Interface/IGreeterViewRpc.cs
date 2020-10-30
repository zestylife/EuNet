using System.Threading.Tasks;

namespace EuNet.Rpc.Test.Interface
{
    public interface IGreeterViewRpc : IViewRpc
    {
        Task<bool> Greet(int value);
    }
}
