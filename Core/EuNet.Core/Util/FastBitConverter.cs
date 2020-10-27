using System.Runtime.InteropServices;

namespace EuNet.Core
{
    public static class FastBitConverter
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct ConverterHelperDouble
        {
            [FieldOffset(0)]
            public ulong Along;

            [FieldOffset(0)]
            public double Adouble;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct ConverterHelperFloat
        {
            [FieldOffset(0)]
            public int Aint;

            [FieldOffset(0)]
            public float Afloat;
        }

        private static void WriteLittleEndian(byte[] buffer, int offset, ulong data)
        {
#if BIGENDIAN
            buffer[offset + 7] = (byte)(data);
            buffer[offset + 6] = (byte)(data >> 8);
            buffer[offset + 5] = (byte)(data >> 16);
            buffer[offset + 4] = (byte)(data >> 24);
            buffer[offset + 3] = (byte)(data >> 32);
            buffer[offset + 2] = (byte)(data >> 40);
            buffer[offset + 1] = (byte)(data >> 48);
            buffer[offset    ] = (byte)(data >> 56);
#else
            buffer[offset] = (byte)(data);
            buffer[offset + 1] = (byte)(data >> 8);
            buffer[offset + 2] = (byte)(data >> 16);
            buffer[offset + 3] = (byte)(data >> 24);
            buffer[offset + 4] = (byte)(data >> 32);
            buffer[offset + 5] = (byte)(data >> 40);
            buffer[offset + 6] = (byte)(data >> 48);
            buffer[offset + 7] = (byte)(data >> 56);
#endif
        }

        private static void WriteLittleEndian(byte[] buffer, int offset, int data)
        {
#if BIGENDIAN
            buffer[offset + 3] = (byte)(data);
            buffer[offset + 2] = (byte)(data >> 8);
            buffer[offset + 1] = (byte)(data >> 16);
            buffer[offset    ] = (byte)(data >> 24);
#else
            buffer[offset] = (byte)(data);
            buffer[offset + 1] = (byte)(data >> 8);
            buffer[offset + 2] = (byte)(data >> 16);
            buffer[offset + 3] = (byte)(data >> 24);
#endif
        }

        public static void WriteLittleEndian(byte[] buffer, int offset, short data)
        {
#if BIGENDIAN
            buffer[offset + 1] = (byte)(data);
            buffer[offset    ] = (byte)(data >> 8);
#else
            buffer[offset] = (byte)(data);
            buffer[offset + 1] = (byte)(data >> 8);
#endif
        }

        public static void GetBytes(byte[] bytes, int startIndex, double value)
        {
            ConverterHelperDouble ch = new ConverterHelperDouble { Adouble = value };
            WriteLittleEndian(bytes, startIndex, ch.Along);
        }

        public static void GetBytes(byte[] bytes, int startIndex, float value)
        {
            ConverterHelperFloat ch = new ConverterHelperFloat { Afloat = value };
            WriteLittleEndian(bytes, startIndex, ch.Aint);
        }

        public static void GetBytes(byte[] bytes, int startIndex, short value)
        {
            WriteLittleEndian(bytes, startIndex, value);
        }

        public static void GetBytes(byte[] bytes, int startIndex, ushort value)
        {
            WriteLittleEndian(bytes, startIndex, (short)value);
        }

        public static void GetBytes(byte[] bytes, int startIndex, int value)
        {
            WriteLittleEndian(bytes, startIndex, value);
        }

        public static void GetBytes(byte[] bytes, int startIndex, uint value)
        {
            WriteLittleEndian(bytes, startIndex, (int)value);
        }

        public static void GetBytes(byte[] bytes, int startIndex, long value)
        {
            WriteLittleEndian(bytes, startIndex, (ulong)value);
        }

        public static void GetBytes(byte[] bytes, int startIndex, ulong value)
        {
            WriteLittleEndian(bytes, startIndex, value);
        }

        public static short ReadInt16LittleEndian(byte[] buffer, int offset)
        {
#if BIGENDIAN
            return (short)(
                (int)buffer[offset + 1] |
                ((int)buffer[offset    ] << 8));
#else
            return (short)(
                (int)buffer[offset] |
                ((int)buffer[offset + 1] << 8));
#endif
        }

        private static int ReadInt32LittleEndian(byte[] buffer, int offset)
        {
#if BIGENDIAN
            return (int)buffer[offset + 3] |
                ((int)buffer[offset + 2] << 8) |
                ((int)buffer[offset + 1] << 16) |
                ((int)buffer[offset    ] << 24);
#else
            return (int)buffer[offset] |
                ((int)buffer[offset + 1] << 8) |
                ((int)buffer[offset + 2] << 16) |
                ((int)buffer[offset + 3] << 24);
#endif
        }

        private static ulong ReadUInt64LittleEndian(byte[] buffer, int offset)
        {
#if BIGENDIAN
            return
                (ulong)buffer[offset + 7] |
                ((ulong)buffer[offset + 6] << 8) |
                ((ulong)buffer[offset + 5] << 16) |
                ((ulong)buffer[offset + 4] << 24) |
                ((ulong)buffer[offset + 3] << 32) |
                ((ulong)buffer[offset + 2] << 40) |
                ((ulong)buffer[offset + 1] << 48) |
                ((ulong)buffer[offset    ] << 56);
#else
            return
                (ulong)buffer[offset] |
                ((ulong)buffer[offset + 1] << 8) |
                ((ulong)buffer[offset + 2] << 16) |
                ((ulong)buffer[offset + 3] << 24) |
                ((ulong)buffer[offset + 4] << 32) |
                ((ulong)buffer[offset + 5] << 40) |
                ((ulong)buffer[offset + 6] << 48) |
                ((ulong)buffer[offset + 7] << 56);
#endif
        }

        public static short ToInt16(byte[] bytes, int startIndex)
        {
            return ReadInt16LittleEndian(bytes, startIndex);
        }

        public static ushort ToUInt16(byte[] bytes, int startIndex)
        {
            return (ushort)ReadInt16LittleEndian(bytes, startIndex);
        }

        public static int ToInt32(byte[] bytes, int startIndex)
        {
            return ReadInt32LittleEndian(bytes, startIndex);
        }

        public static uint ToUInt32(byte[] bytes, int startIndex)
        {
            return (uint)ReadInt32LittleEndian(bytes, startIndex);
        }

        public static long ToInt64(byte[] bytes, int startIndex)
        {
            return (long)ReadUInt64LittleEndian(bytes, startIndex);
        }

        public static ulong ToUInt64(byte[] bytes, int startIndex)
        {
            return ReadUInt64LittleEndian(bytes, startIndex);
        }

        public static float ToSingle(byte[] bytes, int startIndex)
        {
            var value = ReadInt32LittleEndian(bytes, startIndex);
            ConverterHelperFloat ch = new ConverterHelperFloat { Aint = value };
            return ch.Afloat;
        }

        public static double ToDouble(byte[] bytes, int startIndex)
        {
            var value = ReadUInt64LittleEndian(bytes, startIndex);
            ConverterHelperDouble ch = new ConverterHelperDouble { Along = value };
            return ch.Adouble;
        }
    }
}
