namespace EuNet.Core
{
    /// <summary>
    /// 데이터 전송 방법
    /// </summary>
    public enum DeliveryMethod : byte
    {
        /// <summary>
        /// TCP 로 보냄
        /// </summary>
        Tcp = 0,

        /// <summary>
        /// 신뢰할 수 없는 UDP 로 보냄. (소실됨, 중복됨, 순서없음)
        /// </summary>
        Unreliable = 1,

        /// <summary>
        /// 신뢰되지만 순서없이 UDP로 보냄 (소실안됨, 중복안됨, 순서없음)
        /// </summary>
        ReliableUnordered = 2,

        /// <summary>
        /// 신뢰할 수 없는지만 순서대로 UDP로 보냄 (소실됨, 중복안됨, 순서됨)
        /// </summary>
        Sequenced = 3,

        /// <summary>
        /// 신뢰되고 순서를 지키는 UDP로 보냄 (소실안됨, 중복안됨, 순서됨)
        /// </summary>
        ReliableOrdered = 4,

        /// <summary>
        /// 신뢰되고 순서를 지키는 UDP로 보냄 (소실됨[마지막 패킷만 소실안됨], 중복안됨, 순서됨)
        /// </summary>
        ReliableSequenced = 5,

        Max = 6,
    }
}
