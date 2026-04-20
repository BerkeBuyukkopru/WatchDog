using System;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Application.Interfaces.ExternalClients;

namespace Watchdog.Application.UseCases.Auth
{
    public class SendResetCodeUseCase : IUseCaseAsync<string, bool>
    {
        private readonly IAuthRepository _authRepository;

        // DEĞİŞEN KISIM: Mevcut bildirim motorunu (INotificationSender) kullanıyoruz
        private readonly INotificationSender _notificationSender;

        public SendResetCodeUseCase(IAuthRepository authRepository, INotificationSender notificationSender)
        {
            _authRepository = authRepository;
            _notificationSender = notificationSender;
        }

        public async Task<bool> ExecuteAsync(string username)
        {
            var user = await _authRepository.GetByUsernameAsync(username);
            if (user == null) return true; // Güvenlik (User Enumeration açığını kapatmak) için true dönüyoruz.

            // 1. 6 Haneli rastgele kod üret
            var random = new Random();
            var resetCode = random.Next(100000, 999999).ToString();

            // 2. Kodu ve 5 dakikalık süresini veritabanına kaydet
            user.PasswordResetCode = resetCode;
            user.ResetCodeExpiration = DateTime.UtcNow.AddMinutes(5);
            await _authRepository.UpdateUserAsync(user);

            // 3. Super Admin'in belirlediği ve admine miras kalan maile gönder
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2>Watchdog Şifre Sıfırlama Talebi</h2>
                    <p>Merhaba <b>{user.Username}</b>,</p>
                    <p>Giriş şifrenizi yenilemek için doğrulama kodunuz aşağıdadır:</p>
                    <h1 style='color: #2c3e50; letter-spacing: 5px; background: #f8f9fa; padding: 10px; display: inline-block;'>{resetCode}</h1>
                    <p>Bu kodun geçerlilik süresi <b>5 dakikadır</b>. Eğer bu işlemi siz yapmadıysanız, lütfen bu maili dikkate almayın.</p>
                </div>";

            // SENİN MEVCUT MAİL MOTORUNU ÇAĞIRIYORUZ
            await _notificationSender.SendEmailAsync(user.Email, "Watchdog - Şifre Sıfırlama Kodunuz", emailBody);

            return true;
        }
    }
}