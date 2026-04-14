using System.Net;
using System.Text.Json;

namespace Watchdog.Api.Middlewares
{
    // Sistemdeki tüm "Unhandled" (yakalanmamış) hataları tek bir merkezde toplayan süzgeç.
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // İsteği sistemin içine (Controller'lara) gönderiyoruz
                await _next(context);
            }
            catch (Exception ex)
            {
                // Eğer içeride bir hata patlarsa, uygulama çökmeden buraya düşer
                _logger.LogError(ex, "Sistemde beklenmeyen bir hata oluştu!");
                await HandleExceptionAsync(context, ex, _env);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
        {
            context.Response.ContentType = "application/json";

            // Hatanın tipine göre HTTP Status Code belirliyoruz
            context.Response.StatusCode = exception switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
                KeyNotFoundException => (int)HttpStatusCode.NotFound, // 404
                _ => (int)HttpStatusCode.InternalServerError // 500
            };

            // Güvenlik: Canlı ortamda (Production) kod satırlarını göstermiyoruz!
            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = env.IsDevelopment() ? exception.Message : "Sunucu tarafında beklenmeyen bir hata meydana geldi.",
                // Sadece Geliştirme (Development) ortamındaysak hatanın detayını veriyoruz
                details = env.IsDevelopment() ? exception.StackTrace?.ToString() : null
            };

            // JSON formatını React'in sevdiği gibi "camelCase" yapıyoruz
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsync(json);
        }
    }
}
