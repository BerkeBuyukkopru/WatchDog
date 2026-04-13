using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Common;

namespace Watchdog.Domain.Entities
{
    public class AdminUser : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
    }
}
