using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
namespace Services.Hashing
{
    public class PasswordHasher : IPasswordHasher
    {
        
        private const int Iterations = 100_000;
       
        private const int SaltSize = 16;
      
        private const int HashSize = 32;

        public void CreateHash(string password, out byte[] hash, out byte[] salt)
        {
            salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize);
        }

        public bool Verify(string password, byte[] hash, byte[] salt)
        {
            if (salt == null || hash == null || salt.Length != SaltSize || hash.Length != HashSize)
            {
                return false;
            }
            var computedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize);
            return CryptographicOperations.FixedTimeEquals(computedHash, hash);
        }
    }
}
