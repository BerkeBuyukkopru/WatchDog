using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Infrastructure.Auth
{
    // Bu sınıfın tek bir görevi (Single Responsibility) vardır: Şifreleri tek yönlü olarak karmak (Hash).
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            // Kullanıcının girdiği düz metni (plain text) byte dizisine çevir ve SHA-256 ile şifrele
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Byte dizisini veritabanında saklayabileceğimiz uzun bir string (Hex) formatına çevir
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}
