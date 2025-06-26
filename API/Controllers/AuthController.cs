using API.Context;
using API.Models;
//using API.Security;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;
    private readonly UserService _userService;

    public AuthController(AppDbContext context, TokenService tokenService, UserService userService)
    {
        _context = context;
        _tokenService = tokenService;
        _userService = userService;
    }

    //[HttpPost("login")]
    //public async Task<IActionResult> Login([FromBody] LoginRequest request)
    //{
    //    var user = await _context.Users
    //        .Include(u => u.Customer)
    //        .FirstOrDefaultAsync(u => u.Email == request.Email);

    //    if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
    //        return Unauthorized("Kullanıcı adı veya şifre hatalı.");

    //    var token = _tokenService.CreateToken(user);

    //    //var user = User.Identity?.Name ?? "Anonymous";
    //    Log.Information($"Kullanıcı giriş yaptı: {user.Email}", "testUser");
    //    return Ok(new LoginResponse
    //    {
    //        Token = token,
    //        Role = user.Role,
    //        Email = user.Email,
    //        Name= user.Customer?.Name // Bayi adı varsa döndür
    //    });
    //}
    //refresh token eklendi süresi buradan ayarlanıyor
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        var accessToken = _tokenService.CreateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Eski refresh token'ları sil (isteğe bağlı)
        var existingTokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync();

        foreach (var token in existingTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        var newRefreshToken = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        Log.Information($"Kullanıcı giriş yaptı: {user.Email}");

        return Ok(new LoginResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email,
            Name = user.Customer?.Name
        });
    }

    private bool VerifyPassword(string password, string hashed)
    {
        using var sha = SHA256.Create();
        var computed = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return computed == hashed;
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin-sadece")]
    public IActionResult OnlyAdminCanSee()
    {
        return Ok("Bu alan sadece admin içindir.");
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterDealer([FromBody] DealerRegisterDto dto)
    {
        // Aynı kullanıcı adı var mı?
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Bu kullanıcı adı zaten mevcut.");

        var customer = new Customer
        {
            Name = dto.Name,
            CompanyName = dto.CompanyName,
            Phone = dto.Phone,
            Address = dto.Address,
            CityId = dto.CityId,
            DistrictId = dto.DistrictId,
           SalesType = dto.SalesType,
            Users = new List<User>()
        };

        _context.Customers.Add(customer); // User da Customer üzerinden eklenecek
        await _context.SaveChangesAsync();

        var user = await _userService.CreateUserAsync(
           email: dto.Email,
           password: dto.Password,
           role: "dealer", // veya "Dealer"
           customerId: customer.Id
       );


        return Ok(new
        {
            message = "Bayi ve kullanıcı başarıyla kaydedildi.",
            customerId = customer.Id,
            userId = user.Id
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenRefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken && !x.IsRevoked);

        if (storedToken == null || storedToken.Expires < DateTime.UtcNow)
            return Unauthorized("Refresh token is invalid or expired");

        var user = await _context.Users.FindAsync(storedToken.UserId);
        if (user == null) return Unauthorized();

        var newAccessToken = _tokenService.CreateToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(); // yeni token üret

        // Eski refresh token’ı iptal et
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        // Yeni refresh token DB’ye ekle
        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(30)
        });

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken
        });
    }



}
