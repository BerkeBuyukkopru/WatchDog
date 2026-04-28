using System;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Auth
{
    public class ResetPasswordUseCase : IUseCaseAsync<ResetPasswordRequest, bool>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordUseCase(IAuthRepository authRepository, IPasswordHasher passwordHasher)
        {
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> ExecuteAsync(ResetPasswordRequest request)
        {
            var user = await _authRepository.GetByEmailAsync(request.Email);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // 1. Kod boş mu kontrolü
            if (string.IsNullOrEmpty(user.PasswordResetCode))
                throw new Exception("Geçerli bir doğrulama talebi bulunamadı.");

            // 2. Kod doğru mu kontrolü
            if (user.PasswordResetCode != request.ResetCode)
                throw new Exception("Doğrulama kodu hatalı.");

            // 3. Kodun süresi dolmuş mu kontrolü
            if (user.ResetCodeExpiration < DateTime.UtcNow)
                throw new Exception("Doğrulama kodunun süresi dolmuş. Lütfen yeniden kod isteyin.");

            // 4. Doğrulama başarılı! Şifreyi güncelle ve kodları temizle
            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.PasswordResetCode = null; // Kodu yak (Tekrar kullanılamasın)
            user.ResetCodeExpiration = null;

            return await _authRepository.UpdateUserAsync(user);
        }
    }
}