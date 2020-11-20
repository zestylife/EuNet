using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace EuNet.Core
{
    /// <summary>
    /// 스레드 세이프한 오프젝트 풀
    /// </summary>
    /// <typeparam name="T">클래스 객체</typeparam>
    public class ConcurrentObjectPool<T> : IPool
        where T : class, new()
    {
        private ConcurrentQueue<T> _queue;

        private int _alloced;
        private int _total;
        private int _maxCount;

        /// <summary>
        /// 총 할당 횟수
        /// </summary>
        public long TotalAllocCount => _total;

        /// <summary>
        /// 현재 할당된 갯수 (총 할당 횟수 - 총 해제 횟수)
        /// </summary>
        public long AllocCount => _alloced;

        /// <summary>
        /// 현재 풀링되어 있는 객체 갯수
        /// </summary>
        public long PoolingCount => _queue.Count;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="count">미리 할당되어 풀링되는 개수</param>
        /// <param name="maxCount">최대 풀링 개수</param>
        public ConcurrentObjectPool(int count = 0, int maxCount = 1000)
        {
            _queue = new ConcurrentQueue<T>();
            _maxCount = maxCount;

            for (int i = 0; i < count; ++i)
            {
                T obj = new T();
                _queue.Enqueue(obj);
            }
        }

        /// <summary>
        /// 미리 정해준 개수를 풀링시킨다
        /// </summary>
        /// <param name="count">풀링시킬 개수</param>
        public void Prepare(int count)
        {
            int size = count - _queue.Count;
            if (size <= 0)
                return;

            for (int i = 0; i < count; ++i)
            {
                T obj = new T();
                _queue.Enqueue(obj);
            }
        }

        /// <summary>
        /// 객체를 풀에서 빼서 준다.
        /// </summary>
        public virtual T Alloc()
        {
            Interlocked.Increment(ref _total);
            Interlocked.Increment(ref _alloced);

            T obj = null;

            if (_queue.TryDequeue(out obj) == true)
            {
                return obj;
            }

            return new T();
        }

        /// <summary>
        /// 객체를 풀에 넣는다.
        /// </summary>
        /// <param name="obj">풀에 넣을 객체</param>
        public virtual void Free(T obj)
        {
            Interlocked.Decrement(ref _alloced);

            if (_queue.Count >= _maxCount)
                return;

            _queue.Enqueue(obj);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"TotalAllocCount : {TotalAllocCount}");
            builder.AppendLine($"AllocCount : {AllocCount}");
            builder.AppendLine($"PoolingCount : {PoolingCount}");

            return builder.ToString();
        }

    }
}
