using System;
using System.Security.Cryptography;

namespace Istatistik.Services
{
    /// <summary>
    /// ASP.NET Identity (v2 ve v3) PasswordHasher formatı doğrulaması
    /// </summary>
    public static class IdentityPasswordVerifier
    {
        public static bool Verify(string hashedPassword, string password)
        {
            if (string.IsNullOrEmpty(hashedPassword) || password == null)
                return false;

            try
            {
                var data = Convert.FromBase64String(hashedPassword);
                if (data.Length == 0) return false;

                if (data[0] == 0x00)
                    return VerifyV2(data, password);
                if (data[0] == 0x01)
                    return VerifyV3(data, password);
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool VerifyV2(byte[] data, string password)
        {
            if (data.Length != 49) return false;
            var salt = new byte[16];
            Buffer.BlockCopy(data, 1, salt, 0, 16);
            var expected = new byte[32];
            Buffer.BlockCopy(data, 17, expected, 0, 32);

            using (var kdf = new Rfc2898DeriveBytes(password, salt, 1000))
            {
                return FixedTimeEquals(kdf.GetBytes(32), expected);
            }
        }

        private static bool VerifyV3(byte[] data, string password)
        {
            if (data.Length < 13) return false;

            var prf = ReadUInt32(data, 1);
            var iterations = (int)ReadUInt32(data, 5);
            var saltLength = (int)ReadUInt32(data, 9);
            if (saltLength < 16 || data.Length < 13 + saltLength + 1) return false;

            var salt = new byte[saltLength];
            Buffer.BlockCopy(data, 13, salt, 0, saltLength);

            var subkeyLength = data.Length - 13 - saltLength;
            var expected = new byte[subkeyLength];
            Buffer.BlockCopy(data, 13 + saltLength, expected, 0, subkeyLength);

            HashAlgorithmName alg;
            switch (prf)
            {
                case 0: alg = HashAlgorithmName.SHA1; break;
                case 1: alg = HashAlgorithmName.SHA256; break;
                case 2: alg = HashAlgorithmName.SHA512; break;
                default: return false;
            }

            using (var kdf = new Rfc2898DeriveBytes(password, salt, iterations, alg))
            {
                return FixedTimeEquals(kdf.GetBytes(subkeyLength), expected);
            }
        }

        private static uint ReadUInt32(byte[] b, int offset)
        {
            return ((uint)b[offset] << 24) | ((uint)b[offset + 1] << 16) | ((uint)b[offset + 2] << 8) | b[offset + 3];
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            var diff = 0;
            for (var i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
