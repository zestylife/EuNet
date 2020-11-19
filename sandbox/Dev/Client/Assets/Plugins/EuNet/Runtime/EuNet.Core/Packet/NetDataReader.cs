using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("EuNet.Unity")]
namespace EuNet.Core
{
    /// <summary>
    /// 데이터를 읽는 클래스
    /// </summary>
    public class NetDataReader
    {
        protected byte[] _data;
        protected int _position;
        protected int _offset;
        protected int _endOffset;

        /// <summary>
        /// 총 버퍼 크기 (사용되는 양과 관계없음)
        /// </summary>
        public int Capacity
        {
            get { return _data.Length; }
        }

        /// <summary>
        /// 버퍼 배열
        /// </summary>
        public byte[] Data
        {
            get { return _data; }
        }

        /// <summary>
        /// 데이터를 읽을 오프셋. 오프셋부터 데이터를 읽기 시작함.
        /// </summary>
        public int Offset
        {
            get { return _offset; }
        }

        /// <summary>
        /// 버퍼에서 읽을 수 있는 마지막 오프셋
        /// </summary>
        public int EndOffset
        {
            get { return _endOffset; }
        }

        /// <summary>
        /// 읽을 수 있는 총 데이터 사이즈
        /// </summary>
        public int DataSize
        {
            get { return _endOffset - _offset; }
        }

        /// <summary>
        /// 버퍼가 NULL인지 여부
        /// </summary>
        public bool IsNull
        {
            get { return _data == null; }
        }

        /// <summary>
        /// 현재 읽고 있는 Position (offset)
        /// </summary>
        public int Position
        {
            get { return _position; }
            internal set { _position = value; }
        }

        /// <summary>
        /// 읽을 수 있는 마지막 부분에 도달여부
        /// </summary>
        public bool EndOfData
        {
            get { return _position == _endOffset; }
        }

        /// <summary>
        /// 읽을 수 있는 남은 총 바이트
        /// </summary>
        public int AvailableBytes
        {
            get { return _endOffset - _position; }
        }

        public NetDataReader()
        {

        }

        public NetDataReader(NetDataWriter writer)
        {
            SetSource(writer);
        }

        public NetDataReader(byte[] source)
        {
            SetSource(source);
        }

        public NetDataReader(byte[] source, int offset)
        {
            SetSource(source, offset);
        }

        public NetDataReader(byte[] source, int offset, int endOffset)
        {
            SetSource(source, offset, endOffset);
        }

        public NetDataReader(NetPacket packet)
        {
            SetSource(packet.RawData, packet.GetHeaderSize(), packet.Size);
        }

        public void SkipBytes(int count)
        {
            _position += count;
        }

        public void SetSource(NetDataWriter dataWriter)
        {
            _data = dataWriter.Data;
            _position = 0;
            _offset = 0;
            _endOffset = dataWriter.Length;
        }

        public void SetSource(byte[] source)
        {
            _data = source;
            _position = 0;
            _offset = 0;
            _endOffset = source.Length;
        }

        public void SetSource(byte[] source, int offset)
        {
            _data = source;
            _position = offset;
            _offset = offset;
            _endOffset = source.Length;
        }

        public void SetSource(byte[] source, int offset, int maxSize)
        {
            _data = source;
            _position = offset;
            _offset = offset;
            _endOffset = maxSize;
        }

        #region ReadMethods
        
        public byte ReadByte()
        {
            byte res = _data[_position];
            _position += 1;
            return res;
        }

        public sbyte ReadSByte()
        {
            var b = (sbyte)_data[_position];
            _position++;
            return b;
        }

        public bool ReadBoolean()
        {
            bool res = _data[_position] > 0;
            _position += 1;
            return res;
        }

        public char ReadChar()
        {
            char result = BitConverter.ToChar(_data, _position);
            _position += 2;
            return result;
        }

        public short ReadInt16()
        {
            short result = FastBitConverter.ToInt16(_data, _position);
            _position += 2;
            return result;
        }

        public ushort ReadUInt16()
        {
            ushort result = FastBitConverter.ToUInt16(_data, _position);
            _position += 2;
            return result;
        }

        public int ReadInt32()
        {
            int result = FastBitConverter.ToInt32(_data, _position);
            _position += 4;
            return result;
        }

        public uint ReadUInt32()
        {
            uint result = FastBitConverter.ToUInt32(_data, _position);
            _position += 4;
            return result;
        }

        public long ReadInt64()
        {
            long result = FastBitConverter.ToInt64(_data, _position);
            _position += 8;
            return result;
        }

        public ulong ReadUInt64()
        {
            ulong result = FastBitConverter.ToUInt64(_data, _position);
            _position += 8;
            return result;
        }

        public float ReadSingle()
        {
            float result = FastBitConverter.ToSingle(_data, _position);
            _position += 4;
            return result;
        }

        public double ReadDouble()
        {
            double result = FastBitConverter.ToDouble(_data, _position);
            _position += 8;
            return result;
        }

        public IPEndPoint ReadIPEndPoint()
        {
            var addressLen = ReadByte();
            byte[] addressBytes = new byte[addressLen];
            ReadBytesOnlyData(addressBytes, addressLen);
            var port = ReadInt32();
            return new IPEndPoint(new IPAddress(addressBytes), port);
        }

        public DateTime ReadDateTime()
        {
            long ticks = ReadInt64();
            return new DateTime(ticks);
        }

        public TimeSpan ReadTimeSpan()
        {
            long ticks = ReadInt64();
            return new TimeSpan(ticks);
        }

        public Guid ReadGuid()
        {
            byte count = ReadByte();
            var bytes = ReadBytes(count);
            return new Guid(bytes);
        }

        public string ReadString(int maxLength)
        {
            int bytesCount = ReadInt32();
            if (bytesCount <= 0 || bytesCount > maxLength * 2)
            {
                return string.Empty;
            }

            int charCount = Encoding.UTF8.GetCharCount(_data, _position, bytesCount);
            if (charCount > maxLength)
            {
                return string.Empty;
            }

            string result = Encoding.UTF8.GetString(_data, _position, bytesCount);
            _position += bytesCount;
            return result;
        }

        public string ReadString()
        {
            int bytesCount = ReadInt32();
            if (bytesCount <= 0)
            {
                return string.Empty;
            }

            string result = Encoding.UTF8.GetString(_data, _position, bytesCount);
            _position += bytesCount;
            return result;
        }

        public bool[] ReadBooleanArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new bool[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size);
            _position += size;
            return arr;
        }

        public ushort[] ReadUInt16Array()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new ushort[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 2);
            _position += size * 2;
            return arr;
        }

        public short[] ReadInt16Array()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new short[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 2);
            _position += size * 2;
            return arr;
        }

        public long[] ReadInt64Array()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new long[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
            _position += size * 8;
            return arr;
        }

        public ulong[] ReadUInt64Array()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new ulong[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
            _position += size * 8;
            return arr;
        }

        public int[] ReadInt32Array()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new int[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 4);
            _position += size * 4;
            return arr;
        }

        public uint[] ReadUInt32Array()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new uint[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 4);
            _position += size * 4;
            return arr;
        }

        public float[] ReadSingleArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new float[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 4);
            _position += size * 4;
            return arr;
        }

        public double[] ReadDoubleArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new double[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
            _position += size * 8;
            return arr;
        }

        public string[] ReadStringArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new string[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = ReadString();
            }
            return arr;
        }

        public string[] ReadStringArray(int maxStringLength)
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new string[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = ReadString(maxStringLength);
            }
            return arr;
        }

        public ArraySegment<byte> ReadRemainingBytesSegment()
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_data, _position, AvailableBytes);
            _position = _data.Length;
            return segment;
        }

        public T Read<T>() where T : INetSerializable, new()
        {
            var obj = new T();
            obj.Deserialize(this);
            return obj;
        }

        public byte[] ReadRemainingBytes()
        {
            byte[] outgoingData = new byte[AvailableBytes];
            Buffer.BlockCopy(_data, _position, outgoingData, 0, AvailableBytes);
            _position = _data.Length;
            return outgoingData;
        }

        public byte[] ReadBytes(int count)
        {
            var value = new byte[count];
            Buffer.BlockCopy(_data, _position, value, 0, count);
            _position += count;

            return value;
        }

        public byte[] ReadBytes()
        {
            int length = ReadInt32();
            byte[] outgoingData = new byte[length];
            Buffer.BlockCopy(_data, _position, outgoingData, 0, length);
            _position += length;
            return outgoingData;
        }

        public void ReadBytesOnlyData(byte[] destination, int start, int count)
        {
            Buffer.BlockCopy(_data, _position, destination, start, count);
            _position += count;
        }

        public void ReadBytesOnlyData(byte[] destination, int count)
        {
            Buffer.BlockCopy(_data, _position, destination, 0, count);
            _position += count;
        }

        public sbyte[] ReadSBytes()
        {
            int length = ReadInt32();
            sbyte[] outgoingData = new sbyte[length];
            Buffer.BlockCopy(_data, _position, outgoingData, 0, length);
            _position += length;
            return outgoingData;
        }
        
        #endregion

        #region PeekMethods

        public byte PeekByte()
        {
            return _data[_position];
        }

        public sbyte PeekSByte()
        {
            return (sbyte)_data[_position];
        }

        public bool PeekBoolean()
        {
            return _data[_position] > 0;
        }

        public char PeekChar()
        {
            return BitConverter.ToChar(_data, _position);
        }

        public short PeekInt16()
        {
            return BitConverter.ToInt16(_data, _position);
        }

        public ushort PeekUInt16()
        {
            return BitConverter.ToUInt16(_data, _position);
        }

        public int PeekInt32()
        {
            return BitConverter.ToInt32(_data, _position);
        }

        public uint PeekUInt32()
        {
            return BitConverter.ToUInt32(_data, _position);
        }

        public long PeekInt64()
        {
            return BitConverter.ToInt64(_data, _position);
        }

        public ulong PeekUInt64()
        {
            return BitConverter.ToUInt64(_data, _position);
        }

        public float PeekSingle()
        {
            return BitConverter.ToSingle(_data, _position);
        }

        public double PeekDouble()
        {
            return BitConverter.ToDouble(_data, _position);
        }

        public string PeekString(int maxLength)
        {
            int bytesCount = BitConverter.ToInt32(_data, _position);
            if (bytesCount <= 0 || bytesCount > maxLength * 2)
            {
                return string.Empty;
            }

            int charCount = Encoding.UTF8.GetCharCount(_data, _position + 4, bytesCount);
            if (charCount > maxLength)
            {
                return string.Empty;
            }

            string result = Encoding.UTF8.GetString(_data, _position + 4, bytesCount);
            return result;
        }

        public string PeekString()
        {
            int bytesCount = BitConverter.ToInt32(_data, _position);
            if (bytesCount <= 0)
            {
                return string.Empty;
            }

            string result = Encoding.UTF8.GetString(_data, _position + 4, bytesCount);
            return result;
        }
        #endregion

        public bool TryRead<T>(ref T result) where T : INetSerializable
        {
            result.Deserialize(this);
            return true;
        }

        public void Clear()
        {
            _position = 0;
            _endOffset = 0;
            _data = null;
        }
    }
}

