using EuNet.Core;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EuNet.Rpc
{
    public class RpcClassFactory
    {
        private ConcurrentDictionary<int, Func<object, ISession, NetDataReader, NetDataWriter, Task>> _table;

        private static RpcClassFactory _instance;

        public RpcClassFactory()
        {
            _instance = this;
            _table = new ConcurrentDictionary<int, Func<object, ISession, NetDataReader, NetDataWriter, Task>>();
        }

        public static void CheckAndCreateInstance()
        {
            if (_instance == null)
                _instance = new RpcClassFactory();
        }

        public static bool Add(String fullName, Func<object, ISession, NetDataReader, NetDataWriter, Task> func)
        {
            CheckAndCreateInstance();

            return _instance._table.TryAdd(fullName.GetHashCode(), func);
        }

        public static Func<object, ISession, NetDataReader, NetDataWriter, Task> Get(int hashCode)
        {
            CheckAndCreateInstance();

            Func<object, ISession, NetDataReader, NetDataWriter, Task> func;
            _instance._table.TryGetValue(hashCode, out func);
            return func;
        }
    }
}
