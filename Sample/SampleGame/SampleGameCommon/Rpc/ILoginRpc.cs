using EuNet.Core;
using EuNet.Rpc;
using System.Threading.Tasks;

namespace SampleGameCommon
{
    public interface ILoginRpc : IRpc
    {
        Task<int> Login(string id, ISession session);
        Task<UserInfo> GetUserInfo();
    }
}
