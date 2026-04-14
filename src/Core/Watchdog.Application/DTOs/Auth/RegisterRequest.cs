using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Auth
{
    // Yeni bir admin kaydı için gerekli bilgiler.
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
