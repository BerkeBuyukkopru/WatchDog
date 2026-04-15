using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Constants;
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
            // KONTROL: Kullanıcı adı çakışmasını önle.
            if (await _authRepository.IsUsernameExistAsync(request.Username))
            {
                return new RegisterResponse { IsSuccess = false, ErrorMessage = "Bu kullanıcı adı zaten alınmış." };
            }

            var normalizedRole = RoleConstants.NormalizeRole(request.Role);
            if (normalizedRole == null)
            {
                // Geçersiz bir rol (örneğin "Moderator" vs) gönderilmişse reddet.
                return new RegisterResponse { IsSuccess = false, ErrorMessage = $"Geçersiz rol belirtildi. Desteklenen roller: {RoleConstants.SuperAdmin}, {RoleConstants.Admin}." };
            }

            // OLUŞTURMA: Yeni admin nesnesi.
            // Id, CreatedAt ve CreatedBy alanlarını artık elle atamıyoruz (DbContext'e devredildi).
            var newUser = new AdminUser
            {
                Username = request.Username,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = normalizedRole, // Güvenlik kontrolü eklendi

                // YENİ EKLENEN: Gelen listeyi kaydet, eğer liste gelmediyse boş liste ata.
                AllowedAppIds = request.AllowedAppIds ?? new List<Guid>()
            };

            // KAYIT: İşlemi repository üzerinden tamamla.
            var result = await _authRepository.AddUserAsync(newUser); // Repository metot adını kendi projene göre kontrol et (AddUserAsync veya CreateAdminAsync olabilir)

            return result ? new RegisterResponse { IsSuccess = true }
                          : new RegisterResponse { IsSuccess = false, ErrorMessage = "Kayıt sırasında teknik bir hata oluştu." };
        }
    }
}