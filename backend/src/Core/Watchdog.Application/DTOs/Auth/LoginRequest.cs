using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Auth
{
    // Giriş yapmak isteyen kullanıcının gönderdiği ham veriler.
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
