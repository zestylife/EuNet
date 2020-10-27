using EuNet.Rpc;
using System;
using System.Threading.Tasks;

namespace SampleGameCommon
{
    public interface ISecondInterface : IRpc
    {
        Task<int> Second(string name, string msg);
        Task<int> Greet(string name, string msg);
        Task<int> TupleParam(Tuple<int, string> value);
        Task<Tuple<int, string>> TupleReturn();
    }
}
