using EuNet.Core;
using EuNet.Rpc;
using System.Threading.Tasks;

namespace Rpc.Test.Interface
{
    public interface IGreeterRpc : IRpc
    {
        Task<string> Greet(string name);
        Task<DataClass> GreetClass(DataClass dataClass);
        Task<InterfaceSerializeClass> GreetInterfaceSerializeClass(InterfaceSerializeClass dataClass);
        Task<string> SessionParameter(ISession session);
    }
}
