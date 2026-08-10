using System;
using System.Security.Cryptography;
using System.Text;

namespace UniManage.Helpers
{
    // For coursework: Identity handles hashing. This helper is available for any manual hashing needs.
    public static class PasswordHelper
    {
        // SHA256 with salt - not recommended for production (prefer Identity PBKDF2). Provided for demonstration only.
        public static string HashPassword(string password, string salt)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password + salt);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static string GenerateSalt()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }
    }
}