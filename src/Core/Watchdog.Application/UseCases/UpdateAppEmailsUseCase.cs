using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Watchdog.Application.DTOs;
using Watchdog.Application.Interfaces;

namespace Watchdog.Application.UseCases
{
    // İstek olarak UpdateAppEmailsRequest alır, sonuç olarak başarılı olup olmadığını (bool) döner.
    public class UpdateAppEmailsUseCase : IUseCaseAsync<UpdateAppEmailsRequest, (bool IsSuccess, string ErrorMessage)>
    {
        private readonly IMonitoredAppRepository _repository;

        public UpdateAppEmailsUseCase(IMonitoredAppRepository repository)
        {
            _repository = repository;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ExecuteAsync(UpdateAppEmailsRequest request)
        {
            var app = await _repository.GetByIdAsync(request.AppId);
            if (app == null)
            {
                return (false, "Güncellenecek uygulama bulunamadı.");
            }

            // Eğer kullanıcı mailleri tamamen silmek istiyorsa (Fallback'e dönmek için), boş geçmesine izin veriyoruz.
            if (string.IsNullOrWhiteSpace(request.NotificationEmails))
            {
                app.NotificationEmails = string.Empty;
            }
            else
            {
                // Virgül veya noktalı virgülden böl, boşlukları temizle
                var emails = request.NotificationEmails.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(e => e.Trim())
                                                       .ToList();

                // Standart E-Posta Regex'i
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                foreach (var email in emails)
                {
                    if (!regex.IsMatch(email))
                    {
                        // İlk hatalı formatta işlemi durdur ve 400 Bad Request için mesajı yolla
                        return (false, $"Geçersiz e-posta formatı tespit edildi: {email}");
                    }
                }

                // Kullanıcının girdiği dağınık string'i (örn: "a@b.com  ,  c@d.com") tertemiz, standart bir formata sokup kaydediyoruz.
                app.NotificationEmails = string.Join(", ", emails);
            }

            await _repository.UpdateAsync(app);
            return (true, string.Empty);
        }
    }
}
