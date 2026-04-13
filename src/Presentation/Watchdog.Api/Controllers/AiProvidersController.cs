using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;

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

        // PUT: api/AiProviders/{id}/activate
        // DİKKAT: WDG056 ve TTD kuralları gereği, ayar değiştiren uç noktalar sadece Admin yetkisiyle çalışır.
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(Guid id, [FromServices] IUseCaseAsync<Guid, bool> useCase)
        {
            var result = await useCase.ExecuteAsync(id);
            if (result) return Ok(new { message = "Yapay zeka sağlayıcısı başarıyla değiştirildi." });
            return BadRequest(new { message = "Sağlayıcı aktifleştirilirken bir hata oluştu." });
        }

        // PUT: api/AiProviders/{id}
        // Detay güncelleme ucu
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAiProviderRequest dto, [FromServices] IUseCaseAsync<UpdateAiProviderRequest, bool> useCase)
        {
            if (id != dto.Id) return BadRequest(new { message = "ID uyuşmazlığı!" });

            var result = await useCase.ExecuteAsync(dto);
            if (result) return Ok(new { message = "Ayarlar başarıyla kaydedildi." });
            return BadRequest(new { message = "Güncelleme sırasında hata oluştu." });
        }
    }
}
