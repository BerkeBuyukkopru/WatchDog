using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

            // 2. GÜVENLİK: Eğer yeni bir kullanıcı adı gönderilmişse ve bu eskiden farklıysa
            if (!string.IsNullOrWhiteSpace(request.Username) && !admin.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase))
            {
                // Veritabanında bu isimde başka biri var mı diye kontrol et
                var isExist = await _authRepository.IsUsernameExistAsync(request.Username);
                if (isExist)
                {
                    // Eğer isim alınmışsa güvenlik gereği işlemi durdur ve başarısız dön.
                    // (İleride global hata yakalayıcımıza özel bir BusinessException fırlatılabilir).
                    return false;
                }

                admin.Username = request.Username;
            }

            // 3. Eğer yeni bir şifre gönderilmişse hashleyerek güncelle.
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                admin.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            }

            // 4. 🚨 YENİ EKLENEN: Yetkili olduğu uygulamaları (AllowedAppIds) güncelle
            // Eğer arayüzden (request) bir liste gönderilmişse, adminin listesini bununla değiştir.
            // Boş liste gönderilirse tüm yetkileri silinmiş olur. null ise hiç dokunulmaz.
            if (request.AllowedAppIds != null)
            {
                admin.AllowedAppIds = request.AllowedAppIds;
            }

            // 5. 🛡️ SON SUPERADMIN KORUMASI VE ROL GÜNCELLEME
            if (!string.IsNullOrWhiteSpace(request.Role) && !admin.Role.Equals(request.Role, StringComparison.OrdinalIgnoreCase))
            {
                // Eğer mevcut bir SuperAdmin, Admin'e dönüştürülmeye çalışılıyorsa:
                if (admin.Role == Watchdog.Domain.Constants.RoleConstants.SuperAdmin && 
                    request.Role == Watchdog.Domain.Constants.RoleConstants.Admin)
                {
                    var superAdminCount = await _authRepository.GetActiveSuperAdminCountAsync();
                    if (superAdminCount <= 1)
                    {
                        throw new InvalidOperationException("İşlem reddedildi: Sistemde en az bir adet SuperAdmin kalmak zorundadır.");
                    }
                }
                admin.Role = request.Role;
            }

            // 6. Repository üzerinden güncellemeyi tamamla.
            return await _authRepository.UpdateUserAsync(admin);
        }
    }
}