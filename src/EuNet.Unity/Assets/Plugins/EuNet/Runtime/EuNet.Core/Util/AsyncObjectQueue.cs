using System.Collections.Concurrent;

namespace EuNet.Core
{
    public sealed class AsyncObjectQueue<T> : AsyncObject<bool>
    {
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        public ConcurrentQueue<T> Queue => _queue;

        // 모든것을 초기화
        public void Init()
        {
            Clear();
            Reset();
        }

        // 데이터 클리어
        public void Clear()
        {
            _queue.Clear();
        }

        public void Enqueue(T value)
        {
            _queue.Enqueue(value);

            TrySetResult(true);
        }

        public bool TryDequeue(out T value)
        {
            return _queue.TryDequeue(out value);
        }
    }
}
