using EuNet.Rpc;
using System.Threading.Tasks;

namespace StarterCommon
{
    public interface ILoginRpc : IRpc
    {
        Task<int> Login(string id);
        Task<UserInfo> GetUserInfo();
        // Please add user function here
    }
}
