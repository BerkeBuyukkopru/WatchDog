using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
    // Kimlik doğrulama ve Admin yönetimi kurallarını belirleyen sözleşme.
    public interface IAuthRepository
    {
        // Giriş işlemleri için kullanıcı adına göre admin getirir.
        Task<AdminUser?> GetUserByUsernameAsync(string username);

        // Belirli bir admini ID üzerinden bulur.
        Task<AdminUser?> GetByIdAsync(Guid id);

        // Sistemdeki tüm (silinmemiş) adminleri listeler.
        Task<IEnumerable<AdminUser>> GetAllAsync();

        // Kullanıcı adının sistemde benzersiz olup olmadığını kontrol eder.
        Task<bool> IsUsernameExistAsync(string username);

        // Yeni bir admin kaydı oluşturur.
        Task<bool> AddUserAsync(AdminUser user);

        // Mevcut admin bilgilerini günceller.
        Task<bool> UpdateUserAsync(AdminUser user);

        // Admini sistemden (Soft Delete ile) uzaklaştırır.
        Task<bool> DeleteUserAsync(Guid id);
    }
}
