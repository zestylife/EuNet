namespace EuNet.Core
{
    /// <summary>
    /// 풀 인터페이스
    /// </summary>
    public interface IPool
    {
        /// <summary>
        /// 총 할당 횟수
        /// </summary>
        long TotalAllocCount { get; }

        /// <summary>
        /// 현재 할당된 갯수 (총 할당 횟수 - 총 해제 횟수)
        /// </summary>
        long AllocCount { get; }

        /// <summary>
        /// 현재 풀링되어 있는 객체 갯수
        /// </summary>
        long PoolingCount { get; }
    }
}
