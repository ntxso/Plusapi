using API.Context;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;

    public VerificationController(IConfiguration configuration, AppDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    [HttpPost("send-verification-code")]
    public async Task<IActionResult> SendVerificationCode([FromBody] EmailRequest request)
    {
        // Aynı kullanıcı adı var mı?
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Bu kullanıcı adı zaten mevcut.");


        var sendGridApiKey = _configuration["SendGrid:ApiKey"];
        
        if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            return BadRequest("Geçerli bir e-posta adresi giriniz.");

        
        if (string.IsNullOrWhiteSpace(sendGridApiKey))
            return StatusCode(500, "E-posta gönderme servisi yapılandırılamadı. API Anahtarı eksik.");

        var code = GenerateVerificationCode();

        _dbContext.EmailVerifications.Add(new EmailVerification
        {
            Email = request.Email,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        });
        await _dbContext.SaveChangesAsync();

        var client = new SendGridClient(sendGridApiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("noreply@plusaksesuar.com", "Plus Aksesuar"),
            Subject = "Plus Aksesuar E-posta Doğrulama Kodunuz",
            PlainTextContent = $"Merhaba{(string.IsNullOrEmpty(request.Name) ? "" : $" {request.Name}")},\n\n" +
                               $"E-posta adresinizi doğrulamak için doğrulama kodunuz aşağıdadır:\n\n" +
                               $"{code}\n\n" +
                               $"Bu kod {DateTime.Now.AddMinutes(10):HH:mm} (UTC) tarihine kadar geçerlidir.\n" +
                               "Eğer bu talebi siz yapmadıysanız, lütfen bu e-postayı dikkate almayın.\n\n" +
                               "Teşekkürler,\nPlus Aksesuar Ekibi"
        };
        msg.AddTo(new EmailAddress(request.Email));

        var response = await client.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
        {
            return Ok(new { Message = "Doğrulama kodu gönderildi.", ExpiresAt = DateTime.Now.AddMinutes(10) });
        }
        else
        {
            var errorBody = await response.Body.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, $"E-posta gönderilemedi: {response.StatusCode} - {errorBody}");
        }
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        //return Ok("email:"+request.Email);
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("E-posta ve kod gereklidir.");

        var record = _dbContext.EmailVerifications
            .Where(x => x.Email == request.Email && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();
        //return Ok("email:"+request.Email+" code:"+request.Code+" record:"+record?.Code);
        if (record == null || record.CreatedAt.AddMinutes(30) < DateTime.UtcNow)
            return NotFound("Doğrulama kodu süresi dolmuş veya bulunamadı.");

        if (record.Code != request.Code)
            return Unauthorized("Geçersiz doğrulama kodu.");

        record.IsUsed = true;
        await _dbContext.SaveChangesAsync();

        return Ok("E-posta başarıyla doğrulandı.");
    }

    private string GenerateVerificationCode()
    {
        return new Random().Next(100000, 999999).ToString();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public class EmailRequest
    {
        public string Email { get; set; }
        public string? Name { get; set; }
    }

    public class VerifyCodeRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
