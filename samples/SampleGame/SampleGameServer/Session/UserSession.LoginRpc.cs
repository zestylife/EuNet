using EuNet.Core;
using SampleGameCommon;
using System;
using System.Threading.Tasks;

namespace SampleGameServer
{
    public partial class UserSession : ILoginRpc
    {
        private UserInfo _userInfo = new UserInfo();

        public Task<int> Login(string id, ISession session)
        {
            Console.WriteLine($"Received Login request {id}");

            if (id == "AuthedId")
                return Task<int>.FromResult(0);

            return Task<int>.FromResult(1);
        }

        public Task<UserInfo> GetUserInfo()
        {
            _userInfo.Name = "abc";

            return Task<UserInfo>.FromResult(_userInfo);
        }
    }
}
