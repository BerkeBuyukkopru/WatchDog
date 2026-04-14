using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.UseCases.Auth
{
    public class RegisterUseCase : IUseCaseAsync<RegisterRequest, RegisterResponse>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUseCase(IAuthRepository authRepository, IPasswordHasher passwordHasher)
        {
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterResponse> ExecuteAsync(RegisterRequest request)
        {
            // 1. KONTROL: Kullanıcı adı çakışmasını önle.
            if (await _authRepository.IsUsernameExistAsync(request.Username))
            {
                return new RegisterResponse { IsSuccess = false, ErrorMessage = "Bu kullanıcı adı zaten alınmış." };
            }

            // 2. OLUŞTURMA: Yeni admin nesnesi.
            // Id, CreatedAt ve CreatedBy alanlarını artık elle atamıyoruz (DbContext'e devredildi).
            var newUser = new AdminUser
            {
                Username = request.Username,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = "Admin"
            };

            // 3. KAYIT: İşlemi repository üzerinden tamamla.
            var result = await _authRepository.AddUserAsync(newUser);

            return result ? new RegisterResponse { IsSuccess = true }
                          : new RegisterResponse { IsSuccess = false, ErrorMessage = "Kayıt sırasında teknik bir hata oluştu." };
        }
    }
}
