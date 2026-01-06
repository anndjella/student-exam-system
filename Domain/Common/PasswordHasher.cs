using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Domain.Common
{
    public static class PasswordHasher
    {
        public static string Hash(string plain)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
            return Convert.ToBase64String(bytes);
        }
    }
}
