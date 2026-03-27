using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces;
using Watchdog.Application.DTOs;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] //// Dashboard'da "api/SystemConfigurations" adresini açar.
    public class SystemConfigurationsController : ControllerBase
    {
        private readonly ISystemConfigurationService _configService;

        // Kontrolcü sadece arayüzü (Service) tanır, veritabanı detaylarını bilmez.
        public SystemConfigurationsController(ISystemConfigurationService configService)
        {
            _configService = configService;
        }

        [HttpGet] //Mevcut sistem eşiklerini ve AI ayarlarını Dashboard'a gönderir.
        public async Task<IActionResult> Get()
        {
            var config = await _configService.GetConfigAsync();

            if (config == null)
            {
                return NotFound(new { message = "Henüz sistem konfigürasyonu oluşturulmamış." });
            }

            return Ok(config); // 200 OK + JSON Ayar Paketi
        }

        [HttpPost] //Dashboard'dan gelen yeni ayarları kaydeder.
        public async Task<IActionResult> Update([FromBody] SystemConfigDto dto)
        {
            var result = await _configService.UpdateConfigAsync(dto);

            if (result)
            {
                return Ok(new { message = "Sistem ayarları başarıyla güncellendi." });
            }

            return BadRequest(new { message = "Ayarlar güncellenirken veritabanı hatası oluştu." });
        }
    }
}