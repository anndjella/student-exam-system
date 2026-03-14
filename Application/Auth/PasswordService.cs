using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Domain.Entity;
using Microsoft.AspNetCore.Identity;

namespace Application.Auth
{
    public static class PasswordService
    {
        private static readonly PasswordHasher<User> _hasher = new();

        public static string Hash(User user, string plainPassword)
            => _hasher.HashPassword(user, plainPassword);

        public static bool Verify(User user, string plainPassword)
            => _hasher.VerifyHashedPassword(user, user.PasswordHash, plainPassword)
               != PasswordVerificationResult.Failed;
    }
}
