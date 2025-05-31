using API.Context;
using API.Models;
using API.Security;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserService _userService;

    public AuthController(AppDbContext context, TokenService tokenService, UserService userService)
    {
        _context = context;
        _tokenService = tokenService;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        var token = _tokenService.CreateToken(user);

        return Ok(new LoginResponse
        {
            Token = token,
            Role = user.Role,
            Email = user.Email
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


}
