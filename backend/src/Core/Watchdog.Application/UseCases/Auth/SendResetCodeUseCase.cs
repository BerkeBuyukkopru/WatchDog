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
        private readonly INotificationSender _notificationSender;

        public SendResetCodeUseCase(IAuthRepository authRepository, INotificationSender notificationSender)
        {
            _authRepository = authRepository;
            _notificationSender = notificationSender;
        }

        // Parametre "email" olarak güncellendi
        public async Task<bool> ExecuteAsync(string email)
        {
            // Kullanıcıyı artık e-posta adresiyle arıyoruz
            var user = await _authRepository.GetByEmailAsync(email);

            // Güvenlik (User Enumeration açığını kapatmak) için true dönüyoruz.
            if (user == null) return true;

            // 1. 6 Haneli rastgele kod üret
            var random = new Random();
            var resetCode = random.Next(100000, 999999).ToString();

            // 2. Kodu ve 5 dakikalık süresini veritabanına kaydet
            user.PasswordResetCode = resetCode;
            user.ResetCodeExpiration = DateTime.UtcNow.AddMinutes(5);
            await _authRepository.UpdateUserAsync(user);

            // 3. Adminin sistemde kayıtlı e-posta adresine maili gönder
            var emailBody = $@"
                <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f4f7fa; padding: 30px; color: #334155;'>
                    <div style='max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);'>
                        
                        <div style='padding: 20px; border-bottom: 1px solid #f1f5f9; text-align: center;'>
                            <h1 style='margin: 0; color: #4f46e5; font-size: 16px; font-weight: 700; letter-spacing: 1px; text-transform: uppercase;'>WatchDog Güvenlik</h1>
                        </div>

                        <div style='padding: 30px; text-align: center;'>
                            <p style='margin: 0 0 20px 0; font-size: 15px; line-height: 1.5; color: #1e293b;'>
                                Merhaba <strong>{user.Username}</strong>, <br><br>
                                Hesabınız için bir şifre sıfırlama talebi aldık. Giriş şifrenizi yenilemek için kullanmanız gereken kod:
                            </p>

                            <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 20px; margin: 25px 0;'>
                                <span style='font-family: ""Courier New"", Courier, monospace; font-size: 32px; font-weight: 800; letter-spacing: 8px; color: #1e293b;'>{resetCode}</span>
                            </div>

                            <p style='color: #64748b; font-size: 13px; line-height: 1.6; margin-bottom: 0;'>
                                Bu kodun geçerlilik süresi <strong>5 dakikadır</strong>.<br>
                                <span style='font-size: 12px;'>Eğer bu talebi siz yapmadıysanız, hesabınız güvendedir; bu maili görmezden gelebilirsiniz.</span>
                            </p>
                        </div>

                        <div style='padding: 20px; background-color: #f8fafc; text-align: center; border-top: 1px solid #f1f5f9;'>
                            <p style='margin: 0; color: #94a3b8; font-size: 11px;'>
                                &copy; 2026 WatchDog AI Monitoring &bull; Güvenlik Birimi
                            </p>
                        </div>
                    </div>
                </div>";

            // Bildirim motorunu tetikle
            await _notificationSender.SendEmailAsync(user.Email, "Watchdog - Şifre Sıfırlama Kodunuz", emailBody);

            return true;
        }
    }
}