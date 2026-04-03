using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Interfaces.Common;

//Dashboard'dan (React) gelen emirleri karşılayan ve Application katmanına paslar.
namespace Watchdog.Api.Controller
{
    [ApiController] //API kurallarını (validasyon vb.) otomatik işletir.
    [Route("api/[controller]")]
    public class AppsController : ControllerBase
    {
        // Artık constructor'da servis almıyoruz, her metot kendi Use Case'ini FromServices ile çağıracak.
        // Bu sayede Controller inanılmaz hafifleyecek.
        public AppsController() { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] IUseCaseAsync<GetAllAppsRequest, IEnumerable<AppDto>> useCase)
        {
            var apps = await useCase.ExecuteAsync(new GetAllAppsRequest());
            return Ok(apps);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
                    [FromBody] CreateMonitoredAppRequest request,
                    [FromServices] IUseCaseAsync<CreateMonitoredAppRequest, CreateMonitoredAppResponse> useCase)
        {
            var result = await useCase.ExecuteAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
                    Guid id,
                    [FromServices] IUseCaseAsync<DeleteAppRequest, bool> useCase)
        {
            var isDeleted = await useCase.ExecuteAsync(new DeleteAppRequest(id));
            if (!isDeleted)
            {
                return NotFound(new { message = "Silinecek uygulama bulunamadı." });
            }
            return NoContent();
        }

        [HttpPatch("{id:guid}/emails")]
        public async Task<IActionResult> UpdateEmails(
                            Guid id,
                            [FromBody] UpdateAppEmailsRequest request,
                            [FromServices] IUseCaseAsync<UpdateAppEmailsRequest, (bool IsSuccess, string ErrorMessage)> useCase)
        {
            request.AppId = id;
            var result = await useCase.ExecuteAsync(request);

            if (!result.IsSuccess)
            {
                // Eğer hata mesajı 'bulunamadı' kelimesi içeriyorsa 404 dön, yoksa Regex hatasıdır 400 dön.
                if (result.ErrorMessage.Contains("bulunamadı"))
                    return NotFound(new { message = result.ErrorMessage });

                return BadRequest(new { message = result.ErrorMessage });
            }

            return NoContent();
        }
    }
}
