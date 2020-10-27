using EuNet.Rpc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleGameCommon
{
    public interface ITestInterface : IRpc
    {
        Task<int> Greet(string name, string msg);
        Task<int> GreetNoParam();
        Task GreetNoReturn();
        Task GreetClassParam(UserInfo userInfo);
        Task<UserInfo> GreetClassReturn();
        Task GreetArrayParam(int[] array);
        Task<bool[]> GreetArrayReturn();
    }
}
