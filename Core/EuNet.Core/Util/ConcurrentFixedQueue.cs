using System.Threading;

namespace EuNet.Core
{
    public class ConcurrentFixedQueue<T>
        where T : class , new()
    {
        private T[] _arrayItem = new T[0x10000];

        private int _queued = 0;
        private int _front = 0;
        private int _back = 0;

        private const int MaxQueueCount = 0xF000;
        private const uint AndValue = uint.MaxValue >> 16;

        public long Count
        {
            get
            {
                return _queued;
            }
        }

        public long PopedCount
        {
            get
            {
                return MaxQueueCount - _queued;
            }
        }

        public T Pop()
        {
            if (Interlocked.Decrement(ref _queued) < 0)
            {
                Interlocked.Increment(ref _queued);
                return new T();
            }

            uint location = (uint)Interlocked.Increment(ref _front) & AndValue;
            return _arrayItem[location];

        }

        public bool Push(T data)
        {
            if (_queued > MaxQueueCount)
                return false;

            uint location = (uint)Interlocked.Increment(ref _back) & AndValue;
            _arrayItem[location] = data;

            Interlocked.Increment(ref _queued);

            return true;
        }
    }
}
