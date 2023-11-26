using System;
using System.Security.Cryptography;

namespace test.Utility
{
    public class Encrypter
    {
        private const int SaltLength = 16;
        private const int Iterations = 10000;
        private const int HashLength = 32;

        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltLength];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

		public string HashPassword(string password, byte[] salt)
        {
            byte[] hash = GenerateHash(password, salt);
            byte[] saltPlusHash = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, saltPlusHash, 0, salt.Length);
            Array.Copy(hash, 0, saltPlusHash, salt.Length, hash.Length);

            return Convert.ToBase64String(saltPlusHash);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] saltPlusHash = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SaltLength];
            byte[] hash = new byte[HashLength];

            Array.Copy(saltPlusHash, 0, salt, 0, SaltLength);
            Array.Copy(saltPlusHash, SaltLength, hash, 0, HashLength);

            byte[] testHash = GenerateHash(password, salt);

            return SlowEquals(hash, testHash);
        }

        private static byte[] GenerateHash(string password, byte[] salt)
        {
            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                hash = pbkdf2.GetBytes(HashLength);
            }
            return hash;
        }

        public bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }
    }
}