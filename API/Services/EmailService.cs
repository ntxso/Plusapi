using MailKit.Security;
using Microsoft.Extensions.Options; // IOptions arayüzünü kullanmak için bu using'i ekleyin
using MimeKit;
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MimeKit;

/// <summary>
/// E-posta gönderme işlemlerini yöneten servis sınıfı.
/// SMTP ayarlarını IOptions aracılığıyla EmailSettings nesnesinden alır.
/// </summary>
public class EmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    /// <summary>
    /// EmailService'in yeni bir örneğini başlatır.
    /// SMTP ayarları, bağımlılık enjeksiyonu sistemi tarafından sağlanan EmailSettings nesnesinden alınır.
    /// </summary>
    /// <param name="emailSettingsOptions">Yapılandırma ayarlarını içeren IOptions<EmailSettings> nesnesi.</param>
    public EmailService(IOptions<EmailSettings> emailSettingsOptions, ILogger<EmailService> logger)
    {
        // IOptions<EmailSettings> nesnesinin değerini _emailSettings alanına atıyoruz.
        // Bu, appsettings.json veya .env gibi yapılandırma kaynaklarından gelen ayarları içerir.
       
        _emailSettings = emailSettingsOptions.Value;
       
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "EmailService logger cannot be null");
      

    }

    

    public async Task<bool> SendVerificationEmail(string recipientEmail, string verificationCode)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Plus Aksesuar", _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = "E-posta Doğrulama Kodunuz";

            message.Body = new TextPart("html")
            {
                Text = $"Merhaba,<br><br>E-posta doğrulama kodunuz: <b>{verificationCode}</b>"
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                // 465 portu için SecureSocketOptions.SslOnConnect kullanın
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderme hatası");
            return false;
        }
    }
}



/// <summary>
/// E-posta ayarlarını temsil eden model sınıfı.
/// Bu sınıf, appsettings.json veya çevre değişkenlerinden gelen yapılandırmayı bağlamak için kullanılır.
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
    public string SenderEmail { get; set; }
}
