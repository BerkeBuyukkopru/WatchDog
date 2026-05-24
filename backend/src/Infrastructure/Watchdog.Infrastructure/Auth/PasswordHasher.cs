using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Watchdog.Application.Interfaces.Common;
using IdentityPasswordHasher = Microsoft.AspNetCore.Identity.PasswordHasher<object>;
using IdentityPasswordVerificationResult = Microsoft.AspNetCore.Identity.PasswordVerificationResult;

namespace Watchdog.Infrastructure.Auth
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly IdentityPasswordHasher _passwordHasher = new();

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(new object(), password);
        }

        public PasswordVerificationResult VerifyPassword(string passwordHash, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(new object(), passwordHash, providedPassword);

            if (result == IdentityPasswordVerificationResult.Success)
            {
                return PasswordVerificationResult.Success;
            }

            if (result == IdentityPasswordVerificationResult.SuccessRehashNeeded)
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            return IsLegacySha256Hash(passwordHash)
                && string.Equals(passwordHash, ComputeSha256Hash(providedPassword), StringComparison.OrdinalIgnoreCase)
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Failed;
        }

        private static bool IsLegacySha256Hash(string passwordHash)
        {
            return passwordHash.Length == 64 && passwordHash.All(Uri.IsHexDigit);
        }

        private static string ComputeSha256Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}
