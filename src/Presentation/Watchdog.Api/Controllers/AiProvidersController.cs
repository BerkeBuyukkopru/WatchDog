using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Constants;
using Watchdog.Application.UseCases.AI;
using Watchdog.Domain.Entities;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiProvidersController : ControllerBase
    {
        // GET: api/AiProviders (Tüm listeyi döner)
        // Sisteme giriş yapan herkes (Sadece okuma yetkisi) bu listeyi görebilir.
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromServices] IUseCaseAsync<GetAllAiProvidersRequest, IEnumerable<AiProviderDto>> useCase)
        {
            var result = await useCase.ExecuteAsync(new GetAllAiProvidersRequest());
            return Ok(result);
        }

        // POST: Yeni sağlayıcı ekleme SADECE SuperAdmin yetkisindedir.
        [HttpPost]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> Create(
                    [FromBody] CreateAiProviderRequest request,
                    [FromServices] IAiProviderRepository repository)
        {
            var provider = new AiProvider
            {
                Name = request.Name,
                ModelName = request.ModelName,
                ApiUrl = request.ApiUrl,
                ApiKey = request.ApiKey,
                IsActive = false
            };

            var result = await repository.AddAsync(provider);
            if (result) return CreatedAtAction(nameof(GetAll), new { id = provider.Id }, new { message = "Yeni AI sağlayıcısı başarıyla eklendi." });

            return BadRequest(new { message = "Sağlayıcı eklenirken bir hata oluştu." });
        }

        // PUT: api/AiProviders/{id}/activate
        // GÜNCELLEME: Hem SuperAdmin hem de Admin bu işlemi yapabilir.
        [HttpPut("{id}/activate")]
        [Authorize(Roles = RoleConstants.AllAdmins)]
        public async Task<IActionResult> Activate(Guid id, [FromServices] SetActiveAiProviderUseCase useCase)
        {
            var result = await useCase.ExecuteAsync(id);
            if (result) return Ok(new { message = "Yapay zeka sağlayıcısı başarıyla aktif edildi." });

            return BadRequest(new { message = "Sağlayıcı aktifleştirilirken bir hata oluştu. ID bulunamamış olabilir." });
        }

        // PUT: api/AiProviders/{id}
        // Detay güncelleme ucu
        // GÜNCELLEME: SuperAdmin veya Admin tarafından ayarlar güncellenebilir.
        [HttpPut("{id}")]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAiProviderRequest dto, [FromServices] UpdateAiProviderUseCase useCase)
        {
            // KRİTİK DÜZELTME: Swagger/Client tarafındaki ID uyuşmazlığı hatalarını önlemek için, URL'den gelen ID'yi zorla DTO'ya atıyoruz.
            dto.Id = id;

            var result = await useCase.ExecuteAsync(dto);
            if (result) return Ok(new { message = "Ayarlar başarıyla kaydedildi." });

            return BadRequest(new { message = "Güncelleme sırasında hata oluştu. Sağlayıcı bulunamadı." });
        }

        // DELETE: Sağlayıcı silme SADECE SuperAdmin yetkisindedir.
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> Delete(Guid id, [FromServices] IAiProviderRepository repository)
        {
            var result = await repository.DeleteAsync(id);
            if (result) return Ok(new { message = "AI sağlayıcısı sistemden kaldırıldı." });

            return BadRequest(new { message = "Silme işlemi başarısız. Sağlayıcı bulunamadı." });
        }
    }
}