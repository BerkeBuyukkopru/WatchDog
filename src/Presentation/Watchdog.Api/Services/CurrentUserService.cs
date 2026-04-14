using System.Security.Claims;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Token içindeki ID'yi (sub veya NameIdentifier) Guid formatında çeker
        public Guid UserId
        {
            get
            {
                var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");

                if (idClaim != null && Guid.TryParse(idClaim.Value, out var id))
                {
                    return id;
                }

                return Guid.Empty; // Token yoksa veya ID okunamadıysa boş Guid döner
            }
        }

        // Token içindeki kullanıcı adını çeker, bulamazsa senin yazdığın varsayılan "System" değerini kullanır
        public string? Username =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("unique_name")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
            ?? "System";
    }
}