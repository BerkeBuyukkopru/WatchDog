using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Watchdog.Domain.Constants;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Enums;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Sınıfın tamamı için giriş yapmış olma şartı
    public class AppsController : ControllerBase
    {
        public AppsController() { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] IUseCaseAsync<GetAllAppsRequest, IEnumerable<AppDto>> useCase)
        {
            var apps = await useCase.ExecuteAsync(new GetAllAppsRequest());
            return Ok(apps);
        }

        [HttpPost]
        // Hem SuperAdmin hem de normal Admin yeni uygulama ekleyebilir.
        [Authorize(Roles = RoleConstants.AllAdmins)]
        public async Task<IActionResult> Create(
            [FromBody] CreateMonitoredAppRequest request,
            [FromServices] IUseCaseAsync<CreateMonitoredAppRequest, CreateMonitoredAppResponse> useCase)
        {
            var result = await useCase.ExecuteAsync(request);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == AppErrorCode.UrlAlreadyExists)
                {
                    return Conflict(new
                    {
                        errorCode = "URL_ALREADY_EXISTS",
                        message = result.ErrorMessage
                    });
                }
                return BadRequest(new { message = result.ErrorMessage });
            }

            var responsePayload = new
            {
                id = result.Id,
                message = "Uygulama başarıyla eklendi.",
                apiKey = result.ApiKey
            };
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, responsePayload);
        }

        [HttpDelete("{id:guid}")]
        // Hem SuperAdmin hem de normal Admin silebilir.
        [Authorize(Roles = RoleConstants.AllAdmins)]
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
        [Authorize(Roles = RoleConstants.AllAdmins)]
        public async Task<IActionResult> UpdateEmails(
            Guid id,
            [FromBody] UpdateAppEmailsRequest request,
            [FromServices] IUseCaseAsync<UpdateAppEmailsRequest, (bool IsSuccess, string ErrorMessage)> useCase)
        {
            request.AppId = id;
            var result = await useCase.ExecuteAsync(request);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage != null && result.ErrorMessage.Contains("bulunamadı"))
                {
                    return NotFound(new { message = result.ErrorMessage });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            return NoContent();
        }
    }
}