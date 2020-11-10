using System;
using System.Collections.Generic;
using System.Text;

namespace EuNet.Core
{
    /*
        TEA는 uint 4개의 키를 가지는 비밀키 암호화이다.
        암호화 하는 버퍼는 반드시 8의 배수 크기여야 한다.
        한번에 8바이트씩 고속 암호화를 수행하기 때문이다.
    */
    public class CryptTea
    {
        //! 암호화 키 (uint 4개로 구성됨)
        private uint[] _keys;

        public CryptTea(uint[] keys)
        {
            _keys = keys;
        }

        public static uint[] GenerateKey(int seed = 34519)
        {
            var keys = new uint[4];
            int randNext = seed;

            for (int i = 0; i < keys.Length; i++)
            {
                randNext = randNext * 1103515245 + 12345;
                int rand = (randNext / 65536) % 256;
                keys[i] = (byte)rand;
            }

            return keys;
        }

        public int Encrypt(byte[] srcBuffer, int srcOffset, int srcLength)
        {
            return Encrypt(srcBuffer, srcOffset, srcLength, _keys);
        }

        public int Decrypt(byte[] srcBuffer, int srcOffset, int srcLength)
        {
            return Decrypt(srcBuffer, srcOffset, srcLength, _keys);
        }

        public int EncryptSimple(byte[] srcBuffer, int srcOffset, int srcLength)
        {
            return EncryptSimple(srcBuffer, srcOffset, srcLength, _keys);
        }

        public int DecryptSimple(byte[] srcBuffer, int srcOffset, int srcLength)
        {
            return DecryptSimple(srcBuffer, srcOffset, srcLength, _keys);
        }

        static public int Encrypt(byte[] srcBuffer, int srcOffset, int srcLength, uint[] key)
        {
            if (srcLength <= 0 || srcLength % 8 != 0)
                return -1;

            uint delta = 0x9e3779b9;

            uint y = 0;
            uint z = 0;
            uint sum = 0;
            uint n = 0;

            int i;
            int loopCount = srcLength / 8;
            int offset = srcOffset;

            for (i = 0; i < loopCount; ++i)
            {
                y = (uint)(srcBuffer[offset + 3] | srcBuffer[offset + 2] << 8 | srcBuffer[offset + 1] << 16 | srcBuffer[offset] << 24);
                z = (uint)(srcBuffer[offset + 7] | srcBuffer[offset + 6] << 8 | srcBuffer[offset + 5] << 16 | srcBuffer[offset + 4] << 24);

                sum = 0;
                n = 32;

                while (n-- > 0)
                {
                    y += (z << 4 ^ z >> 5) + z ^ sum + key[sum & 3];
                    sum += delta;
                    z += (y << 4 ^ y >> 5) + y ^ sum + key[sum >> 11 & 3];
                }

                srcBuffer[offset] = (byte)((y >> 24) & 0xFF);
                srcBuffer[offset + 1] = (byte)((y >> 16) & 0xFF);
                srcBuffer[offset + 2] = (byte)((y >> 8) & 0xFF);
                srcBuffer[offset + 3] = (byte)((y) & 0xFF);

                srcBuffer[offset + 4] = (byte)((z >> 24) & 0xFF);
                srcBuffer[offset + 5] = (byte)((z >> 16) & 0xFF);
                srcBuffer[offset + 6] = (byte)((z >> 8) & 0xFF);
                srcBuffer[offset + 7] = (byte)((z) & 0xFF);

                offset += 8;
            }

            return srcLength;
        }

        static public int Decrypt(byte[] srcBuffer, int srcOffset, int srcLength, uint[] key)
        {
            if (srcLength <= 0 || srcLength % 8 != 0)
                return -1;

            uint delta = 0x9e3779b9;

            uint y = 0;
            uint z = 0;
            uint sum = 0;
            uint n = 0;

            int i;
            int loopCount = srcLength / 8;
            int offset = srcOffset;

            for (i = 0; i < loopCount; ++i)
            {
                y = (uint)(srcBuffer[offset + 3] | srcBuffer[offset + 2] << 8 | srcBuffer[offset + 1] << 16 | srcBuffer[offset] << 24);
                z = (uint)(srcBuffer[offset + 7] | srcBuffer[offset + 6] << 8 | srcBuffer[offset + 5] << 16 | srcBuffer[offset + 4] << 24);

                sum = delta << 5;
                n = 32;

                while (n-- > 0)
                {
                    z -= (y << 4 ^ y >> 5) + y ^ sum + key[sum >> 11 & 3];
                    sum -= delta;
                    y -= (z << 4 ^ z >> 5) + z ^ sum + key[sum & 3];
                }

                srcBuffer[offset] = (byte)((y >> 24) & 0xFF);
                srcBuffer[offset + 1] = (byte)((y >> 16) & 0xFF);
                srcBuffer[offset + 2] = (byte)((y >> 8) & 0xFF);
                srcBuffer[offset + 3] = (byte)((y) & 0xFF);

                srcBuffer[offset + 4] = (byte)((z >> 24) & 0xFF);
                srcBuffer[offset + 5] = (byte)((z >> 16) & 0xFF);
                srcBuffer[offset + 6] = (byte)((z >> 8) & 0xFF);
                srcBuffer[offset + 7] = (byte)((z) & 0xFF);

                offset += 8;
            }


            return srcLength;
        }

        static public int EncryptSimple(byte[] srcBuffer, int srcOffset, int srcLength, uint[] key)
        {
            if (srcLength <= 0 || srcLength % 8 != 0)
                return -1;

            uint delta = 0x9e3779b9;

            uint y = 0;
            uint z = 0;
            uint sum = 0;
            uint n = 0;

            uint a = key[0];
            uint b = key[1];
            uint c = key[2];
            uint d = key[3];

            int i;
            int loopCount = srcLength / 8;
            int offset = srcOffset;

            bool isLittleEndian = BitConverter.IsLittleEndian;

            for (i = 0; i < loopCount; ++i)
            {
                if (isLittleEndian)
                {
                    y = (uint)(srcBuffer[offset] | srcBuffer[offset + 1] << 8 | srcBuffer[offset + 2] << 16 | srcBuffer[offset + 3] << 24);
                    z = (uint)(srcBuffer[offset + 4] | srcBuffer[offset + 5] << 8 | srcBuffer[offset + 6] << 16 | srcBuffer[offset + 7] << 24);
                }
                else
                {
                    y = (uint)(srcBuffer[offset + 3] | srcBuffer[offset + 2] << 8 | srcBuffer[offset + 1] << 16 | srcBuffer[offset] << 24);
                    z = (uint)(srcBuffer[offset + 7] | srcBuffer[offset + 6] << 8 | srcBuffer[offset + 5] << 16 | srcBuffer[offset + 4] << 24);
                }

                sum = 0;
                n = 4;

                while (n-- > 0)
                {
                    sum += delta;
                    y += (z << 4) + a ^ z + sum ^ (z >> 5) + b;
                    z += (y << 4) + c ^ y + sum ^ (y >> 5) + d;
                }

                if (isLittleEndian)
                {
                    srcBuffer[offset + 3] = (byte)((y >> 24) & 0xFF);
                    srcBuffer[offset + 2] = (byte)((y >> 16) & 0xFF);
                    srcBuffer[offset + 1] = (byte)((y >> 8) & 0xFF);
                    srcBuffer[offset] = (byte)((y) & 0xFF);

                    srcBuffer[offset + 7] = (byte)((z >> 24) & 0xFF);
                    srcBuffer[offset + 6] = (byte)((z >> 16) & 0xFF);
                    srcBuffer[offset + 5] = (byte)((z >> 8) & 0xFF);
                    srcBuffer[offset + 4] = (byte)((z) & 0xFF);
                }
                else
                {
                    srcBuffer[offset] = (byte)((y >> 24) & 0xFF);
                    srcBuffer[offset + 1] = (byte)((y >> 16) & 0xFF);
                    srcBuffer[offset + 2] = (byte)((y >> 8) & 0xFF);
                    srcBuffer[offset + 3] = (byte)((y) & 0xFF);

                    srcBuffer[offset + 4] = (byte)((z >> 24) & 0xFF);
                    srcBuffer[offset + 5] = (byte)((z >> 16) & 0xFF);
                    srcBuffer[offset + 6] = (byte)((z >> 8) & 0xFF);
                    srcBuffer[offset + 7] = (byte)((z) & 0xFF);
                }

                offset += 8;
            }

            return srcLength;
        }

        static public int DecryptSimple(byte[] srcBuffer, int srcOffset, int srcLength, uint[] key)
        {
            if (srcLength <= 0 || srcLength % 8 != 0)
                return -1;

            uint delta = 0x9e3779b9;

            uint y = 0;
            uint z = 0;
            uint sum = 0;
            uint n = 0;

            uint a = key[0];
            uint b = key[1];
            uint c = key[2];
            uint d = key[3];

            int i;
            int loopCount = srcLength / 8;
            int offset = srcOffset;

            bool isLittleEndian = BitConverter.IsLittleEndian;

            for (i = 0; i < loopCount; ++i)
            {
                if (isLittleEndian)
                {
                    y = (uint)(srcBuffer[offset] | srcBuffer[offset + 1] << 8 | srcBuffer[offset + 2] << 16 | srcBuffer[offset + 3] << 24);
                    z = (uint)(srcBuffer[offset + 4] | srcBuffer[offset + 5] << 8 | srcBuffer[offset + 6] << 16 | srcBuffer[offset + 7] << 24);
                }
                else
                {
                    y = (uint)(srcBuffer[offset + 3] | srcBuffer[offset + 2] << 8 | srcBuffer[offset + 1] << 16 | srcBuffer[offset] << 24);
                    z = (uint)(srcBuffer[offset + 7] | srcBuffer[offset + 6] << 8 | srcBuffer[offset + 5] << 16 | srcBuffer[offset + 4] << 24);
                }

                //sum = delta << 5;
                sum = 0x78DDE6E4;
                n = 4;

                while (n-- > 0)
                {
                    z -= (y << 4) + c ^ y + sum ^ (y >> 5) + d;
                    y -= (z << 4) + a ^ z + sum ^ (z >> 5) + b;
                    sum -= delta;
                }

                if (isLittleEndian)
                {
                    srcBuffer[offset + 3] = (byte)((y >> 24) & 0xFF);
                    srcBuffer[offset + 2] = (byte)((y >> 16) & 0xFF);
                    srcBuffer[offset + 1] = (byte)((y >> 8) & 0xFF);
                    srcBuffer[offset] = (byte)((y) & 0xFF);

                    srcBuffer[offset + 7] = (byte)((z >> 24) & 0xFF);
                    srcBuffer[offset + 6] = (byte)((z >> 16) & 0xFF);
                    srcBuffer[offset + 5] = (byte)((z >> 8) & 0xFF);
                    srcBuffer[offset + 4] = (byte)((z) & 0xFF);
                }
                else
                {
                    srcBuffer[offset] = (byte)((y >> 24) & 0xFF);
                    srcBuffer[offset + 1] = (byte)((y >> 16) & 0xFF);
                    srcBuffer[offset + 2] = (byte)((y >> 8) & 0xFF);
                    srcBuffer[offset + 3] = (byte)((y) & 0xFF);

                    srcBuffer[offset + 4] = (byte)((z >> 24) & 0xFF);
                    srcBuffer[offset + 5] = (byte)((z >> 16) & 0xFF);
                    srcBuffer[offset + 6] = (byte)((z >> 8) & 0xFF);
                    srcBuffer[offset + 7] = (byte)((z) & 0xFF);
                }

                offset += 8;
            }

            return srcLength;
        }

        static public long EncryptInt64(long src, uint[] key)
        {
            uint delta = 0x9e3779b9;

            uint y = 0;
            uint z = 0;
            uint sum = 0;
            uint n = 0;

            long result = 0;

            y = (uint)(src >> 32);
            z = (uint)(src);

            sum = 0;
            n = 2;

            while (n-- > 0)
            {
                sum += delta;
                y += (z << 4) + key[0] ^ z + sum ^ (z >> 5) + key[1];
                z += (y << 4) + key[2] ^ y + sum ^ (y >> 5) + key[3];
            }

            result = ((long)y << 32) | (long)z;

            return result;
        }

        static public long DecryptInt64(long src, uint[] key)
        {
            uint delta = 0x9e3779b9;

            uint y = 0;
            uint z = 0;
            uint sum = 0;
            uint n = 0;

            long result = 0;

            y = (uint)(src >> 32);
            z = (uint)(src);

            sum = delta << 1;
            n = 2;

            while (n-- > 0)
            {
                z -= (y << 4) + key[2] ^ y + sum ^ (y >> 5) + key[3];
                y -= (z << 4) + key[0] ^ z + sum ^ (z >> 5) + key[1];
                sum -= delta;
            }

            result = ((long)y << 32) | (long)z;

            return result;
        }

#if FEAT_UNSAFE

        unsafe public static int EncryptSimpleUnsafe(byte* p_pSrcBuffer, byte* p_pDestBuffer, int p_tSrcLen, uint[] p_dwKey)
        {
            //8로 나누어 떨어지지 않으면 하지 않음	
            if (p_tSrcLen <= 0 || p_tSrcLen % 8 != 0)
                return -1;

            uint* pSrc = (uint*)p_pSrcBuffer;
            uint* pDest = (uint*)p_pDestBuffer;

            int tLoop = p_tSrcLen / 8;

            uint y;
            uint z;
            uint sum;
            uint delta = 0x9E3779B9;
            uint a = p_dwKey[0];
            uint b = p_dwKey[1];
            uint c = p_dwKey[2];
            uint d = p_dwKey[3];

            uint n;
            int i;

            for (i = 0; i < tLoop; ++i)
            {
                y = pSrc[0];
                z = pSrc[1];
                sum = 0;
                //n=32;
                n = 4;

                while (n-- > 0)
                {
                    sum += delta;
                    y += (z << 4) + a ^ z + sum ^ (z >> 5) + b;
                    z += (y << 4) + c ^ y + sum ^ (y >> 5) + d;
                }

                pDest[0] = y;
                pDest[1] = z;

                // uint 2개씩 처리
                pSrc += 2;
                pDest += 2;
            }

            return tLoop * 8;
        }

        unsafe public static int DecryptSimpleUnsafe(byte* p_pSrcBuffer, byte* p_pDestBuffer, int p_tSrcLen, uint[] p_dwKey)
        {
            //8로 나누어 떨어지지 않으면 하지 않음	
            if (p_tSrcLen <= 0 || p_tSrcLen % 8 != 0)
                return -1;

            uint* pSrc = (uint*)p_pSrcBuffer;
            uint* pDest = (uint*)p_pDestBuffer;

            int tLoop = p_tSrcLen / 8;

            uint y;
            uint z;
            uint sum;
            uint delta = 0x9e3779b9;
            uint a = p_dwKey[0];
            uint b = p_dwKey[1];
            uint c = p_dwKey[2];
            uint d = p_dwKey[3];
            uint n;
            int i;

            for (i = 0; i < tLoop; ++i)
            {
                y = pSrc[0];
                z = pSrc[1];
                //sum=0xC6EF3720;
                //n=32;
                sum = 0x78DDE6E4;
                n = 4;

                while (n-- > 0)
                {
                    z -= (y << 4) + c ^ y + sum ^ (y >> 5) + d;
                    y -= (z << 4) + a ^ z + sum ^ (z >> 5) + b;
                    sum -= delta;
                }

                pDest[0] = y;
                pDest[1] = z;

                // uint 2개씩 처리
                pSrc += 2;
                pDest += 2;
            }

            return p_tSrcLen;
        }

#endif

    }
}
