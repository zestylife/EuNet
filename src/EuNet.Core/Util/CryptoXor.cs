namespace EuNet.Core
{
    public class CryptXor
    {
        public static byte[] GenerateKey(int seed = 34418, int keyLength = 1024)
        {
            var keys = new byte[keyLength];
            int randNext = seed;

            for (int i = 0; i < keys.Length; i++)
            {
                randNext = randNext * 1103515245 + 12345;
                int rand = (randNext / 65536) % 256;
                keys[i] = (byte)rand;
            }

            return keys;
        }

        public static void Crypt(byte[] buffer, int offset, int size, byte[] key)
        {
            int keyIndex = 0;
            int end = offset + size;
            for (int i = offset; i < end; i++)
            {
                buffer[i] ^= key[keyIndex];
                if (++keyIndex >= key.Length)
                    keyIndex = 0;
            }
        }

#if FEAT_UNSAFE

        unsafe public static void CryptUnsafe(byte* buffer, int size, byte[] key)
        {
            int keyIndex = 0;
            for (int i = 0; i < size; i++)
            {
                buffer[i] ^= key[keyIndex];
                if (++keyIndex >= key.Length)
                    keyIndex = 0;
            }
        }
#endif
    }
}
