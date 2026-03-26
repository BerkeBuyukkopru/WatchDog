using Microsoft.AspNetCore.Mvc;
using Watchdog.Application.DTOs;
using Watchdog.Application.Interfaces;

//Dashboard'dan (React) gelen emirleri karşılayan ve Application katmanına paslar.
namespace Watchdog.Api.Controller
{
    [ApiController] //API kurallarını (validasyon vb.) otomatik işletir.
    [Route("api/[controller]")]
    public class AppsController : ControllerBase
    {
        private readonly IAppService _appService;

        //Constructor Injection.Kontrolcü servisi tanımaz, arayüzü (Interface) tanır.
        public AppsController(IAppService appService)
        {
            _appService = appService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var apps = await _appService.GetAllAppsAsync();
            return Ok(apps); // 200 OK + JSON Veri
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatedAppDto dto)
        {
            // Tuple Deconstruction: Servisten gelen 5'li paketi burada açıyoruz.
            var result = await _appService.AddAppAsync(dto);

            if (!result.IsSuccess)
            {
                // Aynı URL varsa 409 Conflict (Çakışma) fırlatıyoruz.
                if (result.ErrorCode == "URL_ALREADY_EXISTS")
                {
                    return Conflict(new { errorCode = result.ErrorCode, message = result.ErrorMessage });
                }

                // Diğer her türlü hata için 400 Bad Request döneriz
                return BadRequest(new { errorCode = result.ErrorCode, message = result.ErrorMessage });
            }

            // Başarılı kayıtta 201 Created ve "Üretilen API Key" Dashboard'a fırlatılır.
            return Created("", new
            {
                id = result.Id,
                message = "Uygulama başarıyla eklendi.",
                apiKey = result.ApiKey
            });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var isDeleted = await _appService.DeleteAppAsync(id);
            if (!isDeleted)
            {
                return NotFound(new { message = "Silinecek uygulama bulunamadı." });
            }
            // 204 No Content: "Sildim, artık böyle bir şey yok" mesajıdır.
            return NoContent();
        }
    }
}
