using System.Security.Cryptography;
using System.Text;

namespace API.Security
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string hashed)
        {
            return HashPassword(password) == hashed;
        }
    }

}
