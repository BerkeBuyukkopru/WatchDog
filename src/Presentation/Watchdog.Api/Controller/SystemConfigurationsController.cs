using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces;
using Watchdog.Application.DTOs;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemConfigurationsController : ControllerBase
    {
        private readonly ISystemConfigurationService _configService;

        public SystemConfigurationsController(ISystemConfigurationService configService)
        {
            _configService = configService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var config = await _configService.GetConfigAsync();

            if (config == null)
            {
                return NotFound(new { message = "Henüz sistem konfigürasyonu oluşturulmamış." });
            }

            return Ok(config);
        }

        [HttpPost]
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