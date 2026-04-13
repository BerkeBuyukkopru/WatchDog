using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Api.Controllers
{
    [Authorize] // Sadece JWT Token ile giriş yapmış olan yöneticiler erişebilir.
    [ApiController]
    [Route("api/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUseCaseAsync<Guid, bool> _deleteAdminUseCase;
        private readonly IUseCaseAsync<UpdateAdminRequest, bool> _updateUseCase;

        // Dependency Injection: Tüm gerekli servisleri kurumsal yapıda içeri alıyoruz.
        public AdminsController(
            IAuthRepository authRepository,
            IUseCaseAsync<Guid, bool> deleteAdminUseCase,
            IUseCaseAsync<UpdateAdminRequest, bool> updateUseCase)
        {
            _authRepository = authRepository;
            _deleteAdminUseCase = deleteAdminUseCase;
            _updateUseCase = updateUseCase;
        }

        [HttpGet]
        // Sistemdeki aktif (silinmemiş) tüm adminlerin listesini döner.
        public async Task<IActionResult> GetAll()
        {
            var admins = await _authRepository.GetAllAsync();

            // Güvenlik Önlemi: AdminUser entity'sini direkt dönmek yerine anonim bir tip seçiyoruz.
            // Böylece PasswordHash gibi hassas veriler asla API dışına sızmaz.
            var response = admins.Select(a => new
            {
                a.Id,
                a.Username,
                a.Role,
                a.CreatedAt
            });

            return Ok(response);
        }

        [HttpPut]
        // Mevcut bir adminin kullanıcı adını veya şifresini günceller.
        public async Task<IActionResult> Update([FromBody] UpdateAdminRequest request)
        {
            // UpdateAdminUseCase senaryosunu tetikliyoruz.
            var result = await _updateUseCase.ExecuteAsync(request);

            if (result)
                return Ok(new { Message = "Yönetici bilgileri başarıyla güncellendi." });

            return BadRequest("Güncelleme başarısız. Yönetici bulunamadı veya bir hata oluştu.");
        }

        [HttpDelete("{id}")]
        // Belirtilen admini veritabanından yok etmez, Soft Delete ile pasife çeker.
        public async Task<IActionResult> Delete(Guid id)
        {
            // DeleteAdminUseCase senaryosunu tetikliyoruz.
            var result = await _deleteAdminUseCase.ExecuteAsync(id);

            if (result)
                return Ok(new { Message = "Yönetici hesabı başarıyla donduruldu (Soft Delete)." });

            return BadRequest("Yönetici bulunamadı veya işlem sırasında bir hata oluştu.");
        }
    }
}