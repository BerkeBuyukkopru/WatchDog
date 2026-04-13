using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly WatchdogDbContext _context;

        public AuthRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task<AdminUser?> GetUserByUsernameAsync(string username)
        {
            // Login olurken sadece aktif (silinmemiş) olan adminleri dikkate alıyoruz.
            return await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
        }

        public async Task<AdminUser?> GetByIdAsync(Guid id)
        {
            // ID ile arama yaparken silinmiş bir adminin bilgilerine ulaşılamamasını sağlıyoruz.
            return await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<IEnumerable<AdminUser>> GetAllAsync()
        {
            // Dashboard'da listelerken silinmiş adminleri filtreliyoruz.
            return await _context.AdminUsers
                .Where(u => !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsUsernameExistAsync(string username)
        {
            // Yeni kayıt sırasında kullanıcı adı kontrolü (Sadece silinmemişler arasında).
            return await _context.AdminUsers
                .AnyAsync(u => u.Username == username && !u.IsDeleted);
        }

        public async Task<bool> AddUserAsync(AdminUser user)
        {
            await _context.AdminUsers.AddAsync(user);
            // DbContext içindeki SaveChangesAsync, CreatedBy ve CreatedAt alanlarını otomatik doldurur.
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserAsync(AdminUser user)
        {
            _context.AdminUsers.Update(user);
            // DbContext, ModifiedBy ve ModifiedAt alanlarını otomatik olarak günceller.
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.AdminUsers.FindAsync(id);
            // Admin bulunamazsa veya zaten silinmişse işlemi iptal et.
            if (user == null || user.IsDeleted) return false;

            // KRİTİK: Remove metodunu çağırıyoruz. 
            // DbContext'teki interceptor yapımız bunu yakalayıp IsDeleted = true yapacak.
            _context.AdminUsers.Remove(user);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
