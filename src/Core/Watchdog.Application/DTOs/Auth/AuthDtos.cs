using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
