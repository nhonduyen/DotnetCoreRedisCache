﻿using System.Security.Cryptography;
using System.Text;

namespace DotnetCoreRedisCache.Infrastructure.Utility
{
    public class HashGenerator
    {
        public static string GetHash(string plainText)
        {
            string hashText = string.Empty;
            hashText = ComputeSha256Hash(plainText);
            return hashText;
        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (var sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
