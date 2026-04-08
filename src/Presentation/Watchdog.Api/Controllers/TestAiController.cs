using Microsoft.AspNetCore.Mvc;
using Watchdog.Application.Interfaces.ExternalClients;

namespace Watchdog.Api.Controllers
{
    // Ekip Arkadaşıma Not: Bu controller geçicidir. Sadece AI Fabrikamızın (OpenAI / Ollama geçişlerinin) 
    // doğru çalışıp çalışmadığını Swagger üzerinden hızlıca test etmek için oluşturulmuştur.
    [ApiController]
    [Route("api/[controller]")]
    public class TestAiController : ControllerBase
    {
        private readonly IAiClientFactory _aiClientFactory;

        public TestAiController(IAiClientFactory aiClientFactory)
        {
            _aiClientFactory = aiClientFactory;
        }

        [HttpGet("ping-active-ai")]
        public async Task<IActionResult> PingActiveAi()
        {
            try
            {
                // Fabrikadan o anki geçerli AI motorunu istiyoruz
                var aiClient = await _aiClientFactory.CreateClientAsync();

                // Hangi motorun ayağa kalktığını loglamak için tipini alıyoruz
                string activeEngineName = aiClient.GetType().Name;

                // Motora göndereceğimiz basit test komutu
                string testPrompt = "Merhaba, bu bir test mesajıdır. Lütfen sadece 'Bağlantı başarılı Berke, ben [Motor İsmi]' şeklinde tek cümlelik çok kısa bir yanıt ver.";

                // Motoru tetikliyoruz
                var response = await aiClient.AnalyzeAsync(testPrompt);

                return Ok(new
                {
                    Status = "Success",
                    ActiveEngine = activeEngineName,
                    AiResponse = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
        }
    }
}
