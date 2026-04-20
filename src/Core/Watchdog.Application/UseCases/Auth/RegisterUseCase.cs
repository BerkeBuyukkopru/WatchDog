using System;
using System.Collections.Generic;
using System.Linq; // FirstOrDefault vb. için eklendi
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

        // YENİ EKLENEN: Uygulama bilgilerini getirmek için app repository
        private readonly IMonitoredAppRepository _appRepository;

        public RegisterUseCase(
            IAuthRepository authRepository,
            IPasswordHasher passwordHasher,
            IMonitoredAppRepository appRepository) // Inject ettik
        {
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
            _appRepository = appRepository;
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
                return new RegisterResponse { IsSuccess = false, ErrorMessage = $"Geçersiz rol belirtildi. Desteklenen roller: {RoleConstants.SuperAdmin}, {RoleConstants.Admin}." };
            }

            // --- YENİ EKLENEN MANTIK: Uygulamanın e-postasını bul ve miras al ---
            string inheritedEmail = string.Empty;
            var allowedApps = request.AllowedAppIds ?? new List<Guid>();

            if (allowedApps.Any())
            {
                // Adminin yetkilendirildiği ilk uygulamanın bilgilerini çek
                var app = await _appRepository.GetByIdAsync(allowedApps.First());
                if (app != null)
                {
                    // Uygulamanın AdminEmail değerini al
                    inheritedEmail = app.AdminEmail;
                }
            }
            // ---------------------------------------------------------------------

            // OLUŞTURMA: Yeni admin nesnesi.
            var newUser = new AdminUser
            {
                Username = request.Username,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = normalizedRole,

                // YENİ EKLENEN: Miras alınan maili admine kaydet
                Email = inheritedEmail,

                AllowedAppIds = allowedApps
            };

            // KAYIT: İşlemi repository üzerinden tamamla.
            var result = await _authRepository.AddUserAsync(newUser);

            return result ? new RegisterResponse { IsSuccess = true }
                          : new RegisterResponse { IsSuccess = false, ErrorMessage = "Kayıt sırasında teknik bir hata oluştu." };
        }
    }
}