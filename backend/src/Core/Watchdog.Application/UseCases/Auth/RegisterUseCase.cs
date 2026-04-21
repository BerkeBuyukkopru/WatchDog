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

        // NOT: IMonitoredAppRepository silindi, çünkü artık app'den mail çekmiyoruz!

        public RegisterUseCase(
            IAuthRepository authRepository,
            IPasswordHasher passwordHasher)
        {
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterResponse> ExecuteAsync(RegisterRequest request)
        {
            if (await _authRepository.IsUsernameExistAsync(request.Username))
            {
                return new RegisterResponse { IsSuccess = false, ErrorMessage = "Bu kullanıcı adı zaten alınmış." };
            }

            var normalizedRole = RoleConstants.NormalizeRole(request.Role);
            if (normalizedRole == null)
            {
                return new RegisterResponse { IsSuccess = false, ErrorMessage = $"Geçersiz rol belirtildi." };
            }

            var allowedApps = request.AllowedAppIds ?? new List<Guid>();

            // OLUŞTURMA: Yeni admin nesnesi (Artık kendi şahsi maili ile kaydediliyor)
            var newUser = new AdminUser
            {
                Username = request.Username,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = normalizedRole,
                Email = request.Email, // DİREKT FORMDAN GELEN MAİL!
                AllowedAppIds = allowedApps,
                
            };

            var result = await _authRepository.AddUserAsync(newUser);

            return result ? new RegisterResponse { IsSuccess = true }
                          : new RegisterResponse { IsSuccess = false, ErrorMessage = "Kayıt sırasında teknik bir hata oluştu." };
        }
    }
}