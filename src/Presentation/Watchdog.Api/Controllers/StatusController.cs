using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Infrastructure.Persistence;

namespace Watchdog.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatusController : ControllerBase
    {
        private readonly IUseCaseAsync<GetLatestStatusesRequest, IEnumerable<LatestStatusDto>> _getLatestStatusesUseCase;

        // Dependency Injection ile veritabanı bağlantımızı alıyoruz.
        public StatusController(IUseCaseAsync<GetLatestStatusesRequest, IEnumerable<LatestStatusDto>> getLatestStatusesUseCase)
        {
            _getLatestStatusesUseCase = getLatestStatusesUseCase;
        }

        // GET: api/status/history
        [HttpGet("history")]
        public async Task<IActionResult> GetStatusHistory([FromQuery] Guid? appId, [FromQuery] int count = 50)
        {
            // 1. Veritabanına git ve sadece son 'count' (varsayılan 50) logu getir.
            var request = new GetLatestStatusesRequest
            {
                Count = count,
                AppId = appId
            };

            // Veritabanıyla işimiz yok. Sadece Use Case'e "Bana veriyi getir" diyoruz.
            var result = await _getLatestStatusesUseCase.ExecuteAsync(request);

            // 2. Veriyi zaman çizgisine (Timeline) göre grafiğe düzgün basabilmek için
            // React'e gönderirken tekrar eskiden-yeniye doğru sıralıyoruz.
            return Ok(result);
        }
    }
}