using EuNet.Rpc;
using System.Threading.Tasks;

namespace SampleGameCommon.NetViewRpc
{
    public interface IGreetOthers : INetViewRpc
    {
        Task<string> Greet(int count);
    }
}
