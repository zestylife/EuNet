namespace EuNet.Core
{
    public enum DeliveryMethod : byte
    {
        Tcp = 0,

        // 소실됨, 중복됨, 순서없음
        Unreliable = 1,

        // 소실안됨, 중복안됨, 순서없음
        ReliableUnordered = 2,

        // 소실됨, 중복안됨, 순서됨
        Sequenced = 3,

        // 소실안됨, 중복안됨, 순서됨
        ReliableOrdered = 4,

        // 소실됨(마지막 패킷은 소실안됨), 중복안됨, 순서됨
        ReliableSequenced = 5,

        Max = 6,
    }
}
