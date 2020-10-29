using EuNet.Rpc;
using System.Threading.Tasks;

namespace Common
{
    public interface ILoginRpc : IRpc
    {
        Task<int> Login(string id);
        Task<UserInfo> GetUserInfo();
        Task<bool> Join();
        // Please add user function here
    }
}
