using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Auth
{
    // Bir yöneticinin bilgilerini güvenli bir şekilde güncelleyen iş senaryosu.
    public class UpdateAdminUseCase : IUseCaseAsync<UpdateAdminRequest, bool>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UpdateAdminUseCase(IAuthRepository authRepository, IPasswordHasher passwordHasher)
        {
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> ExecuteAsync(UpdateAdminRequest request)
        {
            // 1. Veritabanından ilgili admini bul (Silinmemiş olduğundan emin oluyoruz).
            var admin = await _authRepository.GetByIdAsync(request.Id);

            if (admin == null) return false;

            // 2. Kullanıcı adını güncelle.
            admin.Username = request.Username;

            // 3. Eğer yeni bir şifre gönderilmişse hashleyerek güncelle.
            // Gönderilmemişse mevcut PasswordHash korunur.
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                admin.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            }

            // 4. Repository üzerinden güncellemeyi tamamla.
            // DbContext otomatik olarak ModifiedBy ve ModifiedAt alanlarını dolduracaktır.
            return await _authRepository.UpdateUserAsync(admin);
        }
    }
}
