using Common;
using System;
using System.Threading.Tasks;

namespace StarterServer
{
    public partial class UserSession : ILoginRpc
    {
        public Task<int> Login(string id)
        {
            Console.WriteLine($"Received Login request {id}");

            return Task<int>.FromResult(0);
        }

        public Task<UserInfo> GetUserInfo()
        {
            _userInfo = new UserInfo();
            _userInfo.Name = "abc";

            return Task<UserInfo>.FromResult(_userInfo);
        }

        public Task<bool> Join()
        {
            return Task.FromResult(Server.Instance.P2pGroup.Join(this));
        }
    }
}
