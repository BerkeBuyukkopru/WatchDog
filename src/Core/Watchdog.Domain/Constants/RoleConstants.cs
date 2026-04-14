using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Domain.Constants
{
    // Sistemdeki rol isimlerini tek bir merkezden yönetmek için sabitler sınıfı.
    // Domain katmanında olması, bu rollere projenin her yerinden erişimi sağlar.
    public static class RoleConstants
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";

        // Operasyonel işlemlerde her iki role de yetki vermek için kullanılan birleşik kural.
        public const string AllAdmins = "SuperAdmin,Admin";
    }
}
