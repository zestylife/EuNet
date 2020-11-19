using System;

namespace EuNet.Core
{
    public class NetDataBufferReader : NetDataReader, IDisposable
    {
        public NetDataBufferReader(int size)
            : base(NetPool.BufferPool.Alloc(size))
        {
            
        }

        public NetDataBufferReader(NetDataReader reader)
        {
            int size = reader.AvailableBytes;

            _data = NetPool.BufferPool.Alloc(size);

            reader.ReadBytesOnlyData(_data, size);

            _endOffset = size;
            _offset = 0;
            _position = 0;
        }

        public void Dispose()
        {
            NetPool.BufferPool.Free(_data);
        }
    }
}
