using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DotnetCoreRedisCache.Infrastructure.Utility
{
    public class HashGenerator
    {
        public static string GetHash(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            var hashText = ComputeSha256Hash(json);
            return hashText;
        }

        static string ComputeSha256Hash(string json)
        {
            // Create a SHA256   
            using var sha256Hash = SHA256.Create();

            byte[] bytes = Encoding.UTF8.GetBytes(json);
            byte[] hashBytes = sha256Hash.ComputeHash(bytes);

            // Convert to hex string
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
