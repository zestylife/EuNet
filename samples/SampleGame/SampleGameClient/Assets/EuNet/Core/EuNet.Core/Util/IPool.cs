namespace EuNet.Core
{
    public interface IPool
    {
        // 총 할당 개수
        long TotalAllocCount { get; }

        // 총 할당 개수 - 총 풀링 개수
        long AllocCount { get; }

        // 현재 풀의 개수
        long PoolingCount { get; }
    }
}
