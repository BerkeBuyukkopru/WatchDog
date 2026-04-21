using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Auth; // Sadece senin kendi DTO'ların kullanılacak
using Watchdog.Application.Interfaces.Common;
using Watchdog.Domain.Constants;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
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

        // ---------------- YENİ EKLENEN UÇ NOKTALAR ----------------

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] string username,
            [FromServices] IUseCaseAsync<string, bool> sendResetCodeUseCase)
        {
            await sendResetCodeUseCase.ExecuteAsync(username);
            return Ok(new { Message = "Eğer kullanıcı adı sistemimizde kayıtlıysa, doğrulama kodunuz ilişkili e-posta adresine gönderilmiştir." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            [FromServices] IUseCaseAsync<ResetPasswordRequest, bool> resetPasswordUseCase)
        {
            try
            {
                var success = await resetPasswordUseCase.ExecuteAsync(request);
                if (success)
                {
                    return Ok(new { Message = "Şifreniz başarıyla güncellendi. Giriş sayfasına yönlendiriliyorsunuz..." });
                }
                return BadRequest(new { Message = "İşlem başarısız." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}