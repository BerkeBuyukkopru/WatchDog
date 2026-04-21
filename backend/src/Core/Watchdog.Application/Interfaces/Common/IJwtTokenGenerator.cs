using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Common
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(AdminUser user);
    }
}
