using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.UseCases.AI;

namespace Watchdog.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Sınıfın tamamını korumaya alır
    public class InsightsController : ControllerBase
    {
        private readonly GetAiInsightsUseCase _getInsightsUseCase;
        private readonly GenerateRoutineInsightUseCase _generateInsightUseCase;

        public InsightsController(
            GetAiInsightsUseCase getInsightsUseCase,
            GenerateRoutineInsightUseCase generateInsightUseCase)
        {
            _getInsightsUseCase = getInsightsUseCase;
            _generateInsightUseCase = generateInsightUseCase;
        }

        //Belirli bir uygulama için veya tüm sistemdeki AI tavsiyelerini getirir.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AiInsightDto>>> GetAll([FromQuery] Guid? appId)
        {
            var result = await _getInsightsUseCase.ExecuteAsync(appId);
            return Ok(result);
        }

        //TEST AMAÇLI: Belirli bir uygulama için AI analizini manuel olarak tetikler.
        [HttpPost("analyze/{appId}")]
        public async Task<IActionResult> ManualAnalyze(Guid appId, [FromQuery] int hours = 1)
        {
            var request = new GenerateRoutineInsightRequest
            {
                AppId = appId,
                HoursToAnalyze = hours
            };

            var result = await _generateInsightUseCase.ExecuteAsync(request);

            if (result == null)
                return NotFound("Uygulama bulunamadı veya analiz edilecek veri yok.");

            return Ok(result);
        }

        // Kullanıcı UI üzerinden bir tavsiyeyi 'Çözüldü' olarak işaretler.
        [HttpPatch("{id}/resolve")]
        public async Task<IActionResult> Resolve(Guid id, [FromServices] IUseCaseAsync<Guid, bool> resolveUseCase)
        {
            var result = await resolveUseCase.ExecuteAsync(id);

            if (!result)
                return NotFound(new { message = "İlgili analiz raporu bulunamadı." });

            return Ok(new { message = "Uyarı başarıyla çözüldü olarak işaretlendi." });
        }
    }
}