using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.SystemConfig;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] //// Dashboard'da "api/SystemConfigurations" adresini açar.
    [Authorize]
    public class SystemConfigurationsController : ControllerBase
    {
        // Constructor temizlendi, Controller artık hafif.
        public SystemConfigurationsController() { }

        [HttpGet] //Mevcut sistem eşiklerini ve AI ayarlarını Dashboard'a gönderir.
        public async Task<IActionResult> Get([FromServices] IUseCaseAsync<GetSystemConfigRequest, SystemConfigDto?> useCase)
        {
            var config = await useCase.ExecuteAsync(new GetSystemConfigRequest());

            if (config == null)
            {
                return NotFound(new { message = "Henüz sistem konfigürasyonu oluşturulmamış." });
            }

            return Ok(config); // 200 OK + JSON Ayar Paketi
        }

        [HttpPost] //Dashboard'dan gelen yeni ayarları kaydeder.
        [Authorize(Roles = "Admin")] // Sistem ayarlarını sadece Admin değiştirebilir
        public async Task<IActionResult> Update(
            [FromBody] SystemConfigDto dto,
            [FromServices] IUseCaseAsync<SystemConfigDto, bool> useCase)
        {
            var result = await useCase.ExecuteAsync(dto);

            if (result)
            {
                return Ok(new { message = "Sistem ayarları başarıyla güncellendi." });
            }

            return BadRequest(new { message = "Ayarlar güncellenirken veritabanı hatası oluştu." });
        }
    }
}