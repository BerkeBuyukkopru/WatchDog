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

        // Sadece geçerli rolleri tutan iç liste
        private static readonly string[] ValidRoles = { SuperAdmin, Admin };

        // Dışarıdan gelen (örn: " adMİn ") rol bilgisini kontrol eder.
        // Eğer geçerliyse sistem standardındaki halini (örn: "Admin") döner. Eğer geçersizse null döner.
        public static string? NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return Admin;

            // Önce boşlukları sil
            var cleanRole = role.Trim();

            // OrdinalIgnoreCase yerine, C#'ın Türkçe "İ" veya İngilizce "I" gibi harf farklarını daha iyi tolere etmesi için InvariantCultureIgnoreCase kullanıyoruz.
            var matchedRole = ValidRoles.FirstOrDefault(r =>
                r.Equals(cleanRole, StringComparison.InvariantCultureIgnoreCase));

            // EĞER HALA BULAMADIYSA (Sadece Türkçe 'İ' den dolayı kaçırıyorsa manuel bir temizlik yap)
            if (matchedRole == null)
            {
                // Kullanıcının girdiği kelimedeki Türkçe büyük/küçük İ,ı,I,i karmaşasını İngilizceye çevir.
                var englishSafeRole = cleanRole.Replace("İ", "I").Replace("ı", "i");
                matchedRole = ValidRoles.FirstOrDefault(r =>
                    r.Equals(englishSafeRole, StringComparison.InvariantCultureIgnoreCase));
            }

            return matchedRole;
        }
    }
}
