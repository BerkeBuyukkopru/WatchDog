using Microsoft.AspNetCore.Mvc;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.UseCases.AI;

namespace Watchdog.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
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

        /// <summary>
        /// Belirli bir uygulama için veya tüm sistemdeki AI tavsiyelerini getirir.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AiInsightDto>>> GetAll([FromQuery] Guid? appId)
        {
            var result = await _getInsightsUseCase.ExecuteAsync(appId);
            return Ok(result);
        }

        /// <summary>
        /// TEST AMAÇLI: Belirli bir uygulama için AI analizini manuel olarak tetikler.
        /// </summary>
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
    }
}
