using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.Interfaces.Common
{
    // Bu arayüz, login olan kullanıcının bilgilerine her katmandan (özellikle Infrastructure) güvenli bir şekilde ulaşmamızı sağlar.
    public interface ICurrentUserService
    {
        string? Username { get; }
    }
}
