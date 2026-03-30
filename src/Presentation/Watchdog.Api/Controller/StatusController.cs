using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Watchdog.Infrastructure.Persistence;

namespace Watchdog.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly WatchdogDbContext _context;

        // Dependency Injection ile veritabanı bağlantımızı alıyoruz.
        public StatusController(WatchdogDbContext context)
        {
            _context = context;
        }

        // GET: api/status/history
        [HttpGet("history")]
        public async Task<IActionResult> GetStatusHistory([FromQuery] int count = 50)
        {
            // 1. Veritabanına git ve sadece son 'count' (varsayılan 50) logu getir.
            var history = await _context.HealthSnapshots
                .OrderByDescending(x => x.Timestamp) // En yeniler önce gelsin
                .Take(count)
                .ToListAsync();

            // 2. Veriyi zaman çizgisine (Timeline) göre grafiğe düzgün basabilmek için
            // React'e gönderirken tekrar eskiden-yeniye doğru sıralıyoruz.
            return Ok(history.OrderBy(x => x.Timestamp));
        }
    }
}