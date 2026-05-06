using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchdog.Domain.Constants;
using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Application.UseCases.Auth;

namespace Watchdog.Api.Controllers
{
    // Sınıfa girmek için sadece login olmak (bilet) yeterli. Özel kilitleri metotlara taşıdık.
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly DeleteAdminUseCase _deleteAdminUseCase;
        private readonly IUseCaseAsync<UpdateAdminRequest, bool> _updateUseCase;
        private readonly RestoreAdminUseCase _restoreAdminUseCase;

        // Dependency Injection: Tüm gerekli servisleri kurumsal yapıda içeri alıyoruz.
        public AdminsController(
            IAuthRepository authRepository,
            DeleteAdminUseCase deleteAdminUseCase,
            IUseCaseAsync<UpdateAdminRequest, bool> updateUseCase,
            RestoreAdminUseCase restoreAdminUseCase)
        {
            _authRepository = authRepository;
            _deleteAdminUseCase = deleteAdminUseCase;
            _updateUseCase = updateUseCase;
            _restoreAdminUseCase = restoreAdminUseCase;
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.SuperAdmin)] // Admin hesaplarını sadece SuperAdmin görebilir.
        // Sistemdeki aktif (silinmemiş) tüm adminlerin listesini döner.
        public async Task<IActionResult> GetAll()
        {
            var admins = await _authRepository.GetAllAsync();

            // AdminUser entity'sini direkt dönmek yerine anonim bir tip seçiyoruz.
            var response = admins.Select(a => new
            {
                a.Id,
                a.Username,
                a.Email,
                a.Role,
                a.CreatedAt,
                a.AllowedAppIds
            });

            return Ok(response);
        }

        [HttpPut]
        [Authorize(Roles = RoleConstants.SuperAdmin)] // KİLİT BURADA! Sadece SuperAdmin başkasının bilgilerini güncelleyebilir.
        // Mevcut bir adminin kullanıcı adını veya şifresini günceller.
        public async Task<IActionResult> Update([FromBody] UpdateAdminRequest request)
        {
            // UpdateAdminUseCase senaryosunu tetikliyoruz.
            var result = await _updateUseCase.ExecuteAsync(request);

            if (result)
                return Ok(new { Message = "Yönetici bilgileri başarıyla güncellendi." });

            return BadRequest(new { Message = "Güncelleme başarısız. Yönetici bulunamadı, isim başkası tarafından kullanılıyor veya bir hata oluştu." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleConstants.SuperAdmin)] // KİLİT BURADA! Sadece SuperAdmin başkasını silebilir.
        // Belirtilen admini veritabanından yok etmez, Soft Delete ile pasife çeker.
        public async Task<IActionResult> Delete(Guid id, [FromServices] ICurrentUserService currentUserService)
        {
            // SuperAdmin kendi kendini silemez!
            if (id == currentUserService.UserId)
            {
                return BadRequest(new { Message = "Güvenlik İhlali: Kendi hesabınızı silemezsiniz." });
            }

            try
            {
                var result = await _deleteAdminUseCase.ExecuteAsync(id);

                if (result)
                    return Ok(new { Message = "Yönetici hesabı başarıyla donduruldu (Soft Delete)." });

                return BadRequest(new { Message = $"İşlem başarısız: ID'si {id} olan yönetici bulunamadı veya zaten dondurulmuş." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Sunucu Hatası: {ex.Message}", Detail = ex.InnerException?.Message });
            }
        }

        [HttpPut("profile")]
        [Authorize(Roles = RoleConstants.AllAdmins)] // Hem SuperAdmin Hem Admin kullanabilir
        // Yöneticinin kendi şifresini değiştirmesini sağlar. ID dışarıdan alınmaz.
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateAdminProfileRequest request,
            [FromServices] ICurrentUserService currentUserService)
        {
            // ID'yi kullanıcının gönderdiği JSON'dan DEĞİL, sahtesi yapılamayan JWT Token'ın içinden (CurrentUserService) alıyoruz!
            var myId = currentUserService.UserId;

            if (myId == Guid.Empty)
                return Unauthorized(new { Message = "Kimlik doğrulanamadı." });

            // Var olan Update UseCase'imizi tekrar kullanıyoruz, kod tekrarı yapmıyoruz.
            var updateRequest = new UpdateAdminRequest
            {
                Id = myId,
                Username = string.Empty, // BOŞ GÖNDERİYORUZ! UseCase bunu görünce mevcut ismi koruyacak.
                NewPassword = request.NewPassword
            };

            var result = await _updateUseCase.ExecuteAsync(updateRequest);

            if (result)
                return Ok(new { Message = "Şifreniz başarıyla güncellendi." });

            return BadRequest(new { Message = "Profil güncellenirken bir hata oluştu." });
        }

        // Silinmiş Adminleri Listele
        [HttpGet("deleted")]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> GetDeletedAdmins()
        {
            var deletedAdmins = await _authRepository.GetDeletedAdminsAsync();

            var response = deletedAdmins.Select(a => new
            {
                a.Id,
                a.Username,
                a.Email,
                a.Role,
                a.DeletedAt,
                a.DeletedBy,
                a.AllowedAppIds
            });

            return Ok(response);
        }

        // Silinmiş Admini Geri Getir
        [HttpPost("{id}/restore")]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> Restore(Guid id)
        {
            var result = await _restoreAdminUseCase.ExecuteAsync(id);

            if (result)
                return Ok(new { Message = "Yönetici hesabı başarıyla yeniden aktif edildi." });

            return BadRequest(new { Message = "Yönetici bulunamadı veya zaten aktif durumda." });
        }
    }
}