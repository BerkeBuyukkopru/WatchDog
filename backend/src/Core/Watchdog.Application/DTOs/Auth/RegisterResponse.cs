using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Auth
{
    public class RegisterResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
