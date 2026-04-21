using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        Task<AdminUser?> GetUserByUsernameAsync(string username);

        // === ŞİFRE SIFIRLAMA İÇİN EKLENEN YENİ METOT ===
        Task<AdminUser?> GetByUsernameAsync(string username);

        Task<AdminUser?> GetByIdAsync(Guid id);
        Task<IEnumerable<AdminUser>> GetAllAsync();
        Task<IEnumerable<AdminUser>> GetDeletedAdminsAsync();
        Task<bool> IsUsernameExistAsync(string username);
        Task<bool> AddUserAsync(AdminUser user);
        Task<bool> UpdateUserAsync(AdminUser user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> RestoreUserAsync(Guid id);

        // === YENİ EKLENEN: Uygulamadan sorumlu adminleri bulma metodu ===
        Task<List<AdminUser>> GetAdminsByAppIdAsync(Guid appId);
    }
}