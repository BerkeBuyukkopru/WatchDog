using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            return await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
        }

        public async Task<AdminUser?> GetByUsernameAsync(string username)
        {
            return await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
        }

        // === ŞİFRE SIFIRLAMA İÇİN EKLENEN YENİ METOT ===
        public async Task<AdminUser?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var normalizedEmail = email.Trim().ToLower();

            return await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted);
        }

        public async Task<AdminUser?> GetByIdAsync(Guid id)
        {
            return await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<IEnumerable<AdminUser>> GetAllAsync()
        {
            return await _context.AdminUsers
                .Where(u => !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsUsernameExistAsync(string username)
        {
            var normalizedUsername = username.Trim().ToLower();
            return await _context.AdminUsers
                                 .AnyAsync(u => u.Username.ToLower() == normalizedUsername);
        }

        public async Task<bool> AddUserAsync(AdminUser user)
        {
            await _context.AdminUsers.AddAsync(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserAsync(AdminUser user)
        {
            _context.AdminUsers.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null || user.IsDeleted) return false;

            _context.AdminUsers.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<AdminUser>> GetDeletedAdminsAsync()
        {
            return await _context.AdminUsers
                .Where(u => u.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> RestoreUserAsync(Guid id)
        {
            var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null || !user.IsDeleted) return false;

            user.IsDeleted = false;
            user.DeletedAt = null;
            user.DeletedBy = null;

            _context.AdminUsers.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<AdminUser>> GetAdminsByAppIdAsync(Guid appId)
        {
            return await _context.AdminUsers
                .Where(a => a.AllowedAppIds.Contains(appId) && !a.IsDeleted)
                .ToListAsync();
        }
    }
}