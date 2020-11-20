using System;

namespace EuNet.Unity
{
    /// <summary>
    /// 유니티의 스크립트 실행순서를 자동으로 입력해줌. [ExecutionOrder(0)] 식으로 선언함
    /// </summary>
    public class ExecutionOrder : Attribute
    {
        public int Order;

        public ExecutionOrder(int order)
        {
            Order = order;
        }
    }
}
