using System.Security.Cryptography;
using System.Text;

namespace ApplicationLogic.Utilities;
public static class CustomHasher {

    public static string HashPassword(string password, string salt) {
        var combinedBytes = Encoding.UTF8.GetBytes(salt + password);
        var hashBytes = SHA256.HashData(combinedBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string storedHash, string salt) {
        var hash = HashPassword(password, salt);
        return hash == storedHash;
    }
}