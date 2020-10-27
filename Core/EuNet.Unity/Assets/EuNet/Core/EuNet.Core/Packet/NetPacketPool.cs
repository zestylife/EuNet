using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace EuNet.Core
{
    internal sealed class NetPacketPoolCell : IPool
    {
        private readonly ConcurrentQueue<NetPacket> _queue = new ConcurrentQueue<NetPacket>();
        private readonly int _allocSize;
        private long _totalAllocCount;
        private long _allocCount;

        public int AllocSize => _allocSize;

        public long TotalAllocCount => _totalAllocCount;
        public long AllocCount => _allocCount;
        public long PoolingCount => _queue.Count;

        private int MaxPoolCount = 10000;

        public NetPacketPoolCell(int allocSize)
        {
            _allocSize = allocSize;
        }

        public NetPacket Alloc(int size)
        {
            Interlocked.Increment(ref _totalAllocCount);
            Interlocked.Increment(ref _allocCount);

            NetPacket packet;

            if (_queue.TryDequeue(out packet) == true &&
                packet != null &&
                packet.RawData.Length >= size)
            {
                return packet;
            }

            return new NetPacket(_allocSize);
        }

        public void Free(NetPacket data)
        {
            Interlocked.Decrement(ref _allocCount);

            if (_queue.Count > MaxPoolCount)
                return;

            _queue.Enqueue(data);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"- NetPacketPool[{_allocSize}] -");
            builder.AppendLine($"TotalAllocCount : {TotalAllocCount}");
            builder.AppendLine($"AllocCount : {AllocCount}");
            builder.AppendLine($"PoolingCount : {PoolingCount}");

            return builder.ToString();
        }
    }

    public sealed class NetPacketPool : IPool
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

        private NetPacketPoolCell[] _poolCell = new NetPacketPoolCell[SizeTable.Length];
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

        public NetPacketPool()
        {
            for (int i = 0; i < _poolCell.Length; i++)
                _poolCell[i] = new NetPacketPoolCell(SizeTable[i]);
        }

        public NetPacket Alloc(int size)
        {
            Interlocked.Increment(ref _totalAllocCount);
            Interlocked.Increment(ref _allocCount);

            NetPacket data = null;
            var pool = FindPoolForAlloc(size);
            if (pool != null)
                data = pool.Alloc(size);
            else data = new NetPacket(size);

            data.Size = (ushort)size;
            data.RawData[2] = 0;

            return data;
        }

        public NetPacket Alloc(PacketProperty property, byte[] data, int start, int length)
        {
            int headerSize = NetPacket.GetHeaderSize(property);
            NetPacket packet = Alloc(length + headerSize);
            packet.Property = property;
            Buffer.BlockCopy(data, start, packet.RawData, headerSize, length);
            return packet;
        }

        public NetPacket Alloc(PacketProperty property, NetDataWriter writer)
        {
            return Alloc(property, writer.Data, 0, writer.Length);
        }

        public NetPacket Alloc(PacketProperty property, int size)
        {
            NetPacket packet = Alloc(size + NetPacket.GetHeaderSize(property));
            packet.Property = property;
            return packet;
        }

        public NetPacket Alloc(PacketProperty property)
        {
            NetPacket packet = Alloc(NetPacket.GetHeaderSize(property));
            packet.Property = property;
            return packet;
        }

        public void Free(NetPacket data)
        {
            if (data == null)
                return;

            Interlocked.Decrement(ref _allocCount);

            var pool = FindPoolForFree(data.RawData.Length);
            if (pool == null)
                return;

            pool.Free(data);
        }

        private NetPacketPoolCell FindPoolForAlloc(int size)
        {
            for (int i = 0; i < _poolCell.Length; i++)
            {
                if (size < _poolCell[i].AllocSize)
                    return _poolCell[i];
            }

            return null;
        }

        private NetPacketPoolCell FindPoolForFree(int size)
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

            builder.AppendLine($"- NetPacketPool[Total] -");
            builder.AppendLine($"TotalAllocCount : {TotalAllocCount}");
            builder.AppendLine($"AllocCount : {AllocCount}");
            builder.AppendLine($"PoolingCount : {PoolingCount}");

            foreach (var pool in _poolCell)
                builder.Append(pool.ToString());

            return builder.ToString();
        }
    }
}
