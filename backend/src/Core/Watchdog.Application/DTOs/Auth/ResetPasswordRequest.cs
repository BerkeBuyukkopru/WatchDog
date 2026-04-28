using System.ComponentModel.DataAnnotations;

namespace Watchdog.Application.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Doğrulama kodu 6 haneli olmalıdır.")]
        public string ResetCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}