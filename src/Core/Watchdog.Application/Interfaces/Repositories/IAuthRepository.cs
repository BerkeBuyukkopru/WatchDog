using System;
using System.Collections.Generic;
using System.Threading.Tasks; // 🚨 EKLENDİ: Task asenkron işlemleri için zorunludur.
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
    // Kimlik doğrulama ve Admin yönetimi kurallarını belirleyen sözleşme.
    public interface IAuthRepository
    {
        // Giriş işlemleri için kullanıcı adına göre admin getirir.
        Task<AdminUser?> GetUserByUsernameAsync(string username);

        // Belirli bir admini ID üzerinden bulur. 
        // (GetAllAppsUseCase içinde yetki kontrolü yaparken bu metodu kullanacağız)
        Task<AdminUser?> GetByIdAsync(Guid id);

        // Sistemdeki tüm (silinmemiş) adminleri listeler.
        Task<IEnumerable<AdminUser>> GetAllAsync();

        // Sadece silinmiş adminleri listeler
        Task<IEnumerable<AdminUser>> GetDeletedAdminsAsync();

        // Kullanıcı adının sistemde benzersiz olup olmadığını kontrol eder.
        Task<bool> IsUsernameExistAsync(string username);

        // Yeni bir admin kaydı oluşturur. (AllowedAppIds listesi ile birlikte kaydeder)
        Task<bool> AddUserAsync(AdminUser user);

        // Mevcut admin bilgilerini günceller. 
        // (Gelecekte SuperAdmin bir adminin izlediği uygulamaları değiştirmek isterse bu metot çalışacak)
        Task<bool> UpdateUserAsync(AdminUser user);

        // Admini sistemden (Soft Delete ile) uzaklaştırır.
        Task<bool> DeleteUserAsync(Guid id);

        // Silinmiş bir admini geri getirir (Restore)
        Task<bool> RestoreUserAsync(Guid id);
    }
}