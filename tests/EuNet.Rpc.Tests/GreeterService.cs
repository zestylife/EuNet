using EuNet.Core;
using Rpc.Test.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EuNet.Rpc.Tests
{
    public class GreeterService : GreeterRpcServiceAbstract
    {
        public override Task<string> Greet(string name)
        {
            Console.WriteLine(name);

            return Task<string>.FromResult("Result greet! " + name);
        }

        public override Task<DataClass> GreetClass(DataClass dataClass)
        {
            return Task.FromResult(new DataClass()
            {
                Int = dataClass.Int + 1,
                Property = dataClass.Property + 1,
                String = dataClass.String + "1",
                IgnoreInt = dataClass.IgnoreInt,
                IgnoreProperty = dataClass.IgnoreProperty
            });
        }

        public override Task<Dictionary<int, int>> GreetDictionary(Dictionary<string, string> value)
        {
            throw new NotImplementedException();
        }

        public override Task GreetEnum(DataEnum dataEnum)
        {
            throw new NotImplementedException();
        }

        public override Task<DataEnumForReturn> GreetEnumReturn()
        {
            throw new NotImplementedException();
        }

        public override Task<InterfaceSerializeClass> GreetInterfaceSerializeClass(InterfaceSerializeClass dataClass)
        {
            return Task.FromResult(new InterfaceSerializeClass()
            {
                Int = dataClass.Int + 1,
            });
        }

        public override Task<Tuple<int, int>> GreetTuple(Tuple<string, string> value)
        {
            throw new NotImplementedException();
        }

        public override Task<string> SessionParameter(ISession session)
        {
            return Task.FromResult("SessionParameter");
        }
    }
}