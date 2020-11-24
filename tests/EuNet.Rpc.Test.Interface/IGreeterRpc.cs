using EuNet.Core;
using EuNet.Rpc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rpc.Test.Interface
{
    public interface IGreeterRpc : IRpc
    {
        Task<string> Greet(string name);
        Task<DataClass> GreetClass(DataClass dataClass);
        Task<InterfaceSerializeClass> GreetInterfaceSerializeClass(InterfaceSerializeClass dataClass);
        Task<string> SessionParameter(ISession session);
        Task GreetEnum(DataEnum dataEnum);
        Task<DataEnumForReturn> GreetEnumReturn();
        Task<Tuple<int, int>> GreetTuple(Tuple<string,string> value);
        Task<Dictionary<int, int>> GreetDictionary(Dictionary<string, string> value);
    }
}
