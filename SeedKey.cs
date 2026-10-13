using System;

namespace DensoGen2
{
    /// <summary>IMA tool P3 hash. PIN 5 B + seed 3 B = key 3 B.</summary>
    public static class SeedKey
    {
        const uint N0 = 0xC541A9;
        const uint M = 0x01212050;

        static uint Step(uint n, uint k, int bits)
        {
            for (int i = 0; i < bits; i++, n >>= 1, k >>= 1)
                if (((n ^ k) & 1) != 0)
                    n ^= M;
            return n;
        }

        public static byte[] P3Hash(byte[] pin5, byte[] seed)
        {
            if (pin5 == null || pin5.Length < 5) throw new ArgumentException("PIN 5 baiti");
            if (seed == null || seed.Length < 3) throw new ArgumentException("seed 3 baiti");
            uint n = N0;
            unchecked
            {
                uint k0 = seed[0] | ((uint)seed[1] << 8) | ((uint)seed[2] << 16) | ((uint)pin5[0] << 24);
                n = Step(n, k0, 32);
                n = Step(n, pin5[1], 8);
                n = Step(n, pin5[2], 8);
                n = Step(n, pin5[3], 8);
                n = Step(n, pin5[4], 8);
            }
            uint n0 = n & 0xF, n1 = (n >> 4) & 0xF, n2 = (n >> 8) & 0xF;
            uint n3 = (n >> 12) & 0xF, n4 = (n >> 16) & 0xF, n5 = (n >> 20) & 0xF;
            return new byte[] { (byte)((n2 << 4) | n1), (byte)((n3 << 4) | n5), (byte)((n0 << 4) | n4) };
        }
    }
}
