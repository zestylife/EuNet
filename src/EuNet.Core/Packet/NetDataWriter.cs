using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EuNet.Core
{
    public class NetDataWriter
    {
        protected byte[] _data;
        protected int _position;
        private const int InitialSize = 64;
        private readonly bool _autoResize;

        public int Capacity
        {
            get { return _data.Length; }
        }

        public NetDataWriter() : this(true, InitialSize)
        {
        }

        public NetDataWriter(bool autoResize) : this(autoResize, InitialSize)
        {
        }

        public NetDataWriter(bool autoResize, int initialSize)
        {
            _data = new byte[initialSize];
            _autoResize = autoResize;
        }

        /// <summary>
        /// Creates NetDataWriter from existing ByteArray
        /// </summary>
        /// <param name="bytes">Source byte array</param>
        /// <param name="copy">Copy array to new location or use existing</param>
        public static NetDataWriter FromBytes(byte[] bytes, bool copy)
        {
            if (copy)
            {
                var netDataWriter = new NetDataWriter(true, bytes.Length);
                netDataWriter.WriteOnlyData(bytes);
                return netDataWriter;
            }
            return new NetDataWriter(true, 0) { _data = bytes };
        }

        /// <summary>
        /// Creates NetDataWriter from existing ByteArray (always copied data)
        /// </summary>
        /// <param name="bytes">Source byte array</param>
        /// <param name="offset">Offset of array</param>
        /// <param name="length">Length of array</param>
        public static NetDataWriter FromBytes(byte[] bytes, int offset, int length)
        {
            var netDataWriter = new NetDataWriter(true, bytes.Length);
            netDataWriter.WriteOnlyData(bytes, offset, length);
            return netDataWriter;
        }

        public static NetDataWriter FromString(string value)
        {
            var netDataWriter = new NetDataWriter();
            netDataWriter.Write(value);
            return netDataWriter;
        }

        public void ResizeIfNeed(int newSize)
        {
            int len = _data.Length;
            if (len < newSize)
            {
                while (len < newSize)
                    len *= 2;
                Array.Resize(ref _data, len);
            }
        }

        public void Reset(int size)
        {
            ResizeIfNeed(size);
            _position = 0;
        }

        public void Reset()
        {
            _position = 0;
        }

        public byte[] CopyData()
        {
            byte[] resultData = new byte[_position];
            Buffer.BlockCopy(_data, 0, resultData, 0, _position);
            return resultData;
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public int Length
        {
            get { return _position; }
            internal set { _position = value; }
        }

        public void Write(byte value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 1);
            _data[_position] = value;
            _position++;
        }

        public void Write(sbyte value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 1);
            _data[_position] = (byte)value;
            _position++;
        }

        public void Write(char value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 2);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 2;
        }

        public void Write(short value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 2);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 2;
        }

        public void Write(ushort value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 2);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 2;
        }

        public void Write(int value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 4);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 4;
        }

        public void Write(uint value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 4);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 4;
        }

        public void Write(long value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 8);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 8;
        }

        public void Write(ulong value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 8);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 8;
        }

        public void Write(float value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 4);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 4;
        }

        public void Write(double value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 8);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 8;
        }

        public void WriteOnlyData(byte[] data, int offset, int length)
        {
            if (_autoResize)
                ResizeIfNeed(_position + length);
            Buffer.BlockCopy(data, offset, _data, _position, length);
            _position += length;
        }

        public void WriteOnlyData(byte[] data)
        {
            if (_autoResize)
                ResizeIfNeed(_position + data.Length);
            Buffer.BlockCopy(data, 0, _data, _position, data.Length);
            _position += data.Length;
        }

        public void Write(sbyte[] data, int offset, int length)
        {
            if (_autoResize)
                ResizeIfNeed(_position + length + 4);
            FastBitConverter.GetBytes(_data, _position, length);
            Buffer.BlockCopy(data, offset, _data, _position + 4, length);
            _position += length + 4;
        }

        public void Write(sbyte[] data)
        {
            if (_autoResize)
                ResizeIfNeed(_position + data.Length + 4);
            FastBitConverter.GetBytes(_data, _position, data.Length);
            Buffer.BlockCopy(data, 0, _data, _position + 4, data.Length);
            _position += data.Length + 4;
        }

        public void Write(byte[] data, int offset, int length)
        {
            if (_autoResize)
                ResizeIfNeed(_position + length + 4);
            FastBitConverter.GetBytes(_data, _position, length);
            Buffer.BlockCopy(data, offset, _data, _position + 4, length);
            _position += length + 4;
        }

        public void Write(byte[] data)
        {
            if (_autoResize)
                ResizeIfNeed(_position + data.Length + 4);
            FastBitConverter.GetBytes(_data, _position, data.Length);
            Buffer.BlockCopy(data, 0, _data, _position + 4, data.Length);
            _position += data.Length + 4;
        }

        public void Write(bool value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 1);
            _data[_position] = (byte)(value ? 1 : 0);
            _position++;
        }

        private void Write(Array arr, int sz)
        {
            ushort length = arr == null ? (ushort)0 : (ushort)arr.Length;
            sz *= length;
            if (_autoResize)
                ResizeIfNeed(_position + sz + 2);
            FastBitConverter.GetBytes(_data, _position, length);
            if (arr != null)
                Buffer.BlockCopy(arr, 0, _data, _position + 2, sz);
            _position += sz + 2;
        }

        public void Write(float[] value)
        {
            Write(value, 4);
        }

        public void Write(double[] value)
        {
            Write(value, 8);
        }

        public void Write(long[] value)
        {
            Write(value, 8);
        }

        public void Write(ulong[] value)
        {
            Write(value, 8);
        }

        public void Write(int[] value)
        {
            Write(value, 4);
        }

        public void Write(uint[] value)
        {
            Write(value, 4);
        }

        public void Write(ushort[] value)
        {
            Write(value, 2);
        }

        public void Write(short[] value)
        {
            Write(value, 2);
        }

        public void Write(bool[] value)
        {
            Write(value, 1);
        }

        public void Write(string[] value)
        {
            ushort len = value == null ? (ushort)0 : (ushort)value.Length;
            Write(len);
            for (int i = 0; i < len; i++)
                Write(value[i]);
        }

        public void Write(string[] value, int maxLength)
        {
            ushort len = value == null ? (ushort)0 : (ushort)value.Length;
            Write(len);
            for (int i = 0; i < len; i++)
                Write(value[i], maxLength);
        }

        public void Write(NetDataWriter writer)
        {
            WriteOnlyData(writer.Data, 0, writer.Length);
        }

        public void Write(IPEndPoint endPoint)
        {
            var bytes = endPoint.Address.GetAddressBytes();
            Write((byte)bytes.Length);
            WriteOnlyData(bytes);
            Write(endPoint.Port);
        }

        public void Write(DateTime value)
        {
            Write(value.Ticks);
        }

        public void Write(TimeSpan value)
        {
            Write(value.Ticks);
        }

        public void Write(Guid value)
        {
            var array = value.ToByteArray();

            Write((byte)array.Length);
            WriteOnlyData(array);
        }

        public void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Write(0);
                return;
            }

            //put bytes count
            int bytesCount = Encoding.UTF8.GetByteCount(value);
            if (_autoResize)
                ResizeIfNeed(_position + bytesCount + 4);
            Write(bytesCount);

            //put string
            Encoding.UTF8.GetBytes(value, 0, value.Length, _data, _position);
            _position += bytesCount;
        }

        public void Write(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                Write(0);
                return;
            }

            int length = value.Length > maxLength ? maxLength : value.Length;
            //calculate max count
            int bytesCount = Encoding.UTF8.GetByteCount(value);
            if (_autoResize)
                ResizeIfNeed(_position + bytesCount + 4);

            //put bytes count
            Write(bytesCount);

            //put string
            Encoding.UTF8.GetBytes(value, 0, length, _data, _position);

            _position += bytesCount;
        }

        public void Write<T>(T obj) where T : INetSerializable
        {
            obj.Serialize(this);
        }

        public int GetHashCode(int offset, int length)
        {
            int hc = length;
            for (int i = offset; i < length; ++i)
            {
                hc = unchecked(hc * 314159 + _data[i]);
            }

            return hc;
        }

        public override int GetHashCode()
        {
            int hc = _data.Length;
            for (int i = 0; i < _data.Length; ++i)
            {
                hc = unchecked(hc * 314159 + _data[i]);
            }

            hc = unchecked(hc * 314159 + _position);
            hc = unchecked(hc * 314159 + (_autoResize == true ? 345632 : 57233836));

            return hc;
        }
    }
}
