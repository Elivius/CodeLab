using System;
using System.Security.Cryptography;

namespace CodeLab.Helpers
{
    /// <summary>
    /// Hashes and verifies passwords with PBKDF2 (SHA-256).
    /// Stored format: PBKDF2$iterations$saltBase64$hashBase64
    /// </summary>
    public static class PasswordHelper
    {
        private const string Prefix = "PBKDF2";
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }

            byte[] hash = ComputeHash(password, salt, Iterations, HashSize);
            return Prefix + "$" + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            string[] parts = storedHash.Split('$');
            int iterations;
            if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out iterations))
            {
                return false;
            }

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actualHash = ComputeHash(password, salt, iterations, expectedHash.Length);
            return AreEqual(actualHash, expectedHash);
        }

        private static byte[] ComputeHash(string password, byte[] salt, int iterations, int hashSize)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(hashSize);
            }
        }

        // Compares every byte regardless of where the first difference is, to avoid leaking timing information
        private static bool AreEqual(byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            int difference = 0;
            for (int index = 0; index < first.Length; index++)
            {
                difference |= first[index] ^ second[index];
            }

            return difference == 0;
        }
    }
}
