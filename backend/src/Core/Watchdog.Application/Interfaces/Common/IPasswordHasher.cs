using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.Interfaces.Common
{
    public enum PasswordVerificationResult
    {
        Failed = 0,
        Success = 1,
        SuccessRehashNeeded = 2
    }

    // Sistemdeki tüm şifreleme işlemlerinin tek bir merkezden yönetilmesi için sözleşme (Interface).
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        PasswordVerificationResult VerifyPassword(string passwordHash, string providedPassword);
    }
}
