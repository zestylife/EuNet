using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace EuNet.Core
{
    internal sealed class BufferPoolCell : IPool
    {
        private readonly ConcurrentQueue<byte[]> _queue;
        private readonly int _allocSize;
        private long _totalAllocCount;
        private long _allocCount;
        private int _maxPoolCount;

        public int AllocSize => _allocSize;

        public long TotalAllocCount => _totalAllocCount;
        public long AllocCount => _allocCount;
        public long PoolingCount => _queue.Count;

        public BufferPoolCell(int allocSize, int maxPoolCount)
        {
            _allocSize = allocSize;
            _maxPoolCount = maxPoolCount;

            _queue = new ConcurrentQueue<byte[]>();
        }

        public byte[] Alloc(int size)
        {
            Interlocked.Increment(ref _totalAllocCount);
            Interlocked.Increment(ref _allocCount);

            byte[] buffer;

            if (_queue.TryDequeue(out buffer) &&
                buffer != null &&
                buffer.Length >= size)
            {
                return buffer;
            }

            return new byte[_allocSize];
        }

        public void Free(byte[] data)
        {
            Interlocked.Decrement(ref _allocCount);

            if (_queue.Count > _maxPoolCount)
                return;

            _queue.Enqueue(data);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"- BufferPool[{_allocSize}] -");
            builder.AppendLine($"TotalAllocCount : {TotalAllocCount}");
            builder.AppendLine($"AllocCount : {AllocCount}");
            builder.AppendLine($"PoolingCount : {PoolingCount}");

            return builder.ToString();
        }
    }

    public sealed class BufferPool : IPool
    {
        // 사이즈테이블. 최대 사이즈가 넘어가면 풀링하지 않음
        private static readonly int[] SizeTable =
        {
            32,
            64,
            128,
            256,
            512,
            1024,
            2048,
            4096,
        };

        private BufferPoolCell[] _poolCell = new BufferPoolCell[SizeTable.Length];
        private long _totalAllocCount;
        private long _allocCount;

        public long TotalAllocCount => _totalAllocCount;
        public long AllocCount => _allocCount;
        public long PoolingCount
        {
            get
            {
                long result = 0;
                foreach (var pool in _poolCell)
                    result += pool.PoolingCount;
                return result;
            }
        }

        public BufferPool(int maxPoolCountPerSegment = 1000)
        {
            for (int i = 0; i < _poolCell.Length; i++)
                _poolCell[i] = new BufferPoolCell(SizeTable[i], maxPoolCountPerSegment);
        }

        public byte[] Alloc(int size)
        {
            Interlocked.Increment(ref _totalAllocCount);
            Interlocked.Increment(ref _allocCount);

            byte[] data = null;
            var pool = FindPoolForAlloc(size);
            if (pool != null)
                data = pool.Alloc(size);
            else data = new byte[size];

            return data;
        }

        public void Free(byte[] data)
        {
            if (data == null)
                return;

            Interlocked.Decrement(ref _allocCount);

            var pool = FindPoolForFree(data.Length);
            if (pool == null)
                return;

            pool.Free(data);
        }

        private BufferPoolCell FindPoolForAlloc(int size)
        {
            for (int i = 0; i < _poolCell.Length; i++)
            {
                if (size < _poolCell[i].AllocSize)
                    return _poolCell[i];
            }

            return null;
        }

        private BufferPoolCell FindPoolForFree(int size)
        {
            for (int i = _poolCell.Length - 1; i >= 0; i--)
            {
                if (size >= _poolCell[i].AllocSize)
                    return _poolCell[i];
            }

            return null;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"- BufferPool[Total] -");
            builder.AppendLine($"TotalAllocCount : {TotalAllocCount}");
            builder.AppendLine($"AllocCount : {AllocCount}");
            builder.AppendLine($"PoolingCount : {PoolingCount}");

            foreach (var pool in _poolCell)
                builder.Append(pool.ToString());

            return builder.ToString();
        }
    }
}
