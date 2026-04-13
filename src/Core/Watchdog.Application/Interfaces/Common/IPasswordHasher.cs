using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.Interfaces.Common
{
    // Sistemdeki tüm şifreleme işlemlerinin tek bir merkezden yönetilmesi için sözleşme (Interface).
    public interface IPasswordHasher
    {
        string HashPassword(string password);
    }
}
