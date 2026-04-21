using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Auth
{
    // Giriş başarılıysa frontend'e döneceğimiz cevap paketi.
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
