using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchdog.Domain.Constants; //Sabitleri ekledik
using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous] // Kilitli kapıların dışındaki tek açık gişe
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            [FromServices] IUseCaseAsync<LoginRequest, LoginResponse> loginUseCase)
        {
            var result = await loginUseCase.ExecuteAsync(request);

            if (!result.IsSuccess)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            return Ok(new { token = result.Token });
        }

        [HttpPost("register")]
        // Artık Magic String yok ve sisteme yeni kişileri SADECE SuperAdmin ekleyebilir!
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            [FromServices] IUseCaseAsync<RegisterRequest, RegisterResponse> registerUseCase)
        {
            var result = await registerUseCase.ExecuteAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Yeni yönetici başarıyla eklendi." });
        }
    }
}