using System;
using System.Security.Cryptography;
using System.Text;

namespace ApplicationLogic.Utilities
{
    public class CustomHasher {
        private const string GlobalSalt = "VeljaNeZnaSaSaltStaDaRadi"; 

        
        public static string HashPassword(string password) {
            using (var sha256 = SHA256.Create()) {
                var combinedBytes = Encoding.UTF8.GetBytes(GlobalSalt + password);
                var hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        
        public static bool VerifyPassword(string password, string storedHash) {
            var hash = HashPassword(password); 
            return hash == storedHash;        
        }
    }
}