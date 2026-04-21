using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Watchdog.Application.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol zorunludur.")]
        public string Role { get; set; } = string.Empty;

        // YENİ EKLENEN: Her adminin kendi şahsi maili olacak.
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        public List<Guid>? AllowedAppIds { get; set; }
    }
}