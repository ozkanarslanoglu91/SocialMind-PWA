using System.Net;
using System.Net.Mail;

namespace SocialMind.Web.Services;

public interface IEmailService
{
    Task<bool> SendWelcomeEmailAsync(string email, string displayName);
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
    Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken);
    Task<bool> SendInvoiceEmailAsync(string email, string invoiceUrl, decimal amount, string planName);
    Task<bool> SendGenericEmailAsync(string to, string subject, string htmlBody);
}

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string displayName)
    {
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center;'>
                    <h1 style='color: white; margin: 0;'>🌐 SocialMind'e Hoş Geldiniz!</h1>
                </div>
                <div style='padding: 40px 20px;'>
                    <p>Merhaba <strong>{displayName}</strong>,</p>
                    <p>SocialMind'e kayıt olduğunuz için teşekkür ederiz! Sosyal medya yönetimi artık hiç bu kadar kolay olmamıştı.</p>
                    
                    <div style='background: #f5f5f5; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h2 style='color: #667eea;'>Başlangıç Rehberi:</h2>
                        <ol>
                            <li><strong>Hesapları Bağla:</strong> Instagram, YouTube, TikTok hesaplarınızı bağlayın</li>
                            <li><strong>İlk Gönderinizi Oluşturun:</strong> AI ile geliştirilmiş gönderi rekomendasyonları alın</li>
                            <li><strong>Zamanlama:</strong> Gönderiyi en uygun saatlerde paylaşmasını planlayın</li>
                            <li><strong>Analitik:</strong> Tüm platformlardaki performansı takip edin</li>
                        </ol>
                    </div>

                    <div style='text-align: center; margin-top: 30px;'>
                        <a href='https://localhost:7259/connected-accounts' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block;'>
                            Hesapları Bağlamaya Başla
                        </a>
                    </div>

                    <p style='margin-top: 40px; color: #999; font-size: 12px;'>
                        Sorularınız mı var? <a href='mailto:support@socialmind.app' style='color: #667eea;'>Destek Ekibiyle İletişime Geçin</a>
                    </p>
                </div>
            </div>";

        return await SendGenericEmailAsync(email, "SocialMind'e Hoş Geldiniz! 🎉", htmlBody);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var resetLink = $"https://localhost:7259/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(email)}";

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px 20px; text-align: center;'>
                    <h1 style='color: white; margin: 0;'>🔐 Şifre Sıfırlama</h1>
                </div>
                <div style='padding: 40px 20px;'>
                    <p>Şifrenizi sıfırlama talebinde bulundunuz. Aşağıdaki butona tıklayarak yeni bir şifre oluşturabilirsiniz.</p>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block;'>
                            Şifremi Sıfırla
                        </a>
                    </div>

                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Bu link 24 saat içinde geçerlidir. Eğer bu talebini siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.
                    </p>

                    <p style='color: #999; font-size: 11px; text-align: center; margin-top: 40px;'>
                        © 2026 SocialMind. Tüm hakları saklıdır.
                    </p>
                </div>
            </div>";

        return await SendGenericEmailAsync(email, "Şifre Sıfırlama Talebiniz", htmlBody);
    }

    public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken)
    {
        var confirmLink = $"https://localhost:7259/confirm-email?token={Uri.EscapeDataString(confirmationToken)}&email={Uri.EscapeDataString(email)}";

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px 20px; text-align: center;'>
                    <h1 style='color: white; margin: 0;'>✉️ E-posta Doğrulama</h1>
                </div>
                <div style='padding: 40px 20px;'>
                    <p>SocialMind'e hoş geldiniz! E-posta adresinizi doğrulamak için lütfen aşağıdaki butona tıklayın.</p>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmLink}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block;'>
                            E-postamı Doğrula
                        </a>
                    </div>

                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Eğer bu talebini siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.
                    </p>
                </div>
            </div>";

        return await SendGenericEmailAsync(email, "E-posta Doğrulaması Gerekli", htmlBody);
    }

    public async Task<bool> SendInvoiceEmailAsync(string email, string invoiceUrl, decimal amount, string planName)
    {
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px 20px; text-align: center;'>
                    <h1 style='color: white; margin: 0;'>🧾 Ödeme Makbuzu</h1>
                </div>
                <div style='padding: 40px 20px;'>
                    <div style='background: #f5f5f5; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 0; color: #666;'>Plan: <strong>{planName}</strong></p>
                        <p style='margin: 10px 0 0 0; font-size: 24px; color: #667eea;'>${amount:F2}</p>
                    </div>

                    <p>Ödemeniz başarıyla işlendi. Faturanızı aşağıdaki bağlantıdan indirebilirsiniz.</p>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{invoiceUrl}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block;'>
                            Faturayı İndir
                        </a>
                    </div>

                    <p style='color: #999; font-size: 12px; margin-top: 40px;'>
                        Sorularınız mı var? <a href='mailto:billing@socialmind.app' style='color: #667eea;'>Faturalandırma Ekibiyle İletişime Geçin</a>
                    </p>
                </div>
            </div>";

        return await SendGenericEmailAsync(email, $"{planName} Planı - Ödeme Makbuzu", htmlBody);
    }

    public async Task<bool> SendGenericEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            var smtpHost = _config["Email:SmtpHost"];
            var smtpPort = _config.GetValue<int>("Email:SmtpPort", 587);
            var smtpUsername = _config["Email:SmtpUsername"];
            var smtpPassword = _config["Email:SmtpPassword"];
            var enableSsl = _config.GetValue<bool>("Email:EnableSsl", true);
            var fromAddress = _config["Email:FromAddress"];
            var fromName = _config["Email:FromName"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername))
            {
                _logger.LogWarning("SMTP yapılandırması eksik. E-posta gönderilemedi: {To}", to);
                return false;
            }

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.Timeout = 30000;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromAddress, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("E-posta başarıyla gönderildi: {To} - {Subject}", to, subject);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderimi başarısız: {To} - {Subject}", to, subject);
            return false;
        }
    }
}
