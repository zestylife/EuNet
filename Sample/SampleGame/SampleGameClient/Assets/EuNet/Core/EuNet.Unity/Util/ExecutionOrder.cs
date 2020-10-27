using System;

namespace EuNet.Unity
{
    public class ExecutionOrder : Attribute
    {
        public int Order;

        public ExecutionOrder(int order)
        {
            Order = order;
        }
    }
}
