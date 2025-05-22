using API.Context;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        var token = _tokenService.CreateToken(user);

        return Ok(new LoginResponse
        {
            Token = token,
            Role = user.Role,
            Username = user.Username
        });
    }

    private bool VerifyPassword(string password, string hashed)
    {
        using var sha = SHA256.Create();
        var computed = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return computed == hashed;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-sadece")]
    public IActionResult OnlyAdminCanSee()
    {
        return Ok("Bu alan sadece admin içindir.");
    }

}
