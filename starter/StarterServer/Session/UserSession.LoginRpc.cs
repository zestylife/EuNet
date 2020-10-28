using StarterCommon;
using System;
using System.Threading.Tasks;

namespace StarterServer
{
    public partial class UserSession : ILoginRpc
    {
        public Task<int> Login(string id)
        {
            Console.WriteLine($"Received Login request {id}");

            if (id == "AuthedId")
                return Task<int>.FromResult(0);

            return Task<int>.FromResult(1);
        }

        public Task<UserInfo> GetUserInfo()
        {
            _userInfo = new UserInfo();
            _userInfo.Name = "abc";

            return Task<UserInfo>.FromResult(_userInfo);
        }
    }
}
