using API.Models;
using API.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Services;

public class TokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Email,user.Email),
            new Claim("CustomerId", user.CustomerId?.ToString() ?? "0")

        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    //public async Task<(string Token, string RefreshToken)> CreateTokenAsync(User user)
    //{
    //    var claims = new List<Claim>
    //{
    //    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    //    new Claim(JwtRegisteredClaimNames.Email, user.Email),
    //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    //    new Claim(ClaimTypes.Role, user.Role),
    //    new Claim("CustomerId", user.CustomerId?.ToString() ?? "0")
    //};

    //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

    //    var token = new JwtSecurityToken(
    //        issuer: _config["Jwt:Issuer"],
    //        audience: _config["Jwt:Audience"],
    //        claims: claims,
    //        expires: DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:TokenExpiryMinutes", 180)), // Varsayılan 3 saat
    //        notBefore: DateTime.UtcNow,
    //        signingCredentials: creds
    //    );

    //    var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
    //    var refreshToken = GenerateRefreshToken();

    //    await _userService.SaveRefreshTokenAsync(user.Id, refreshToken,
    //        DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7)));

    //    return (jwtToken, refreshToken);
    //}

    //private static string GenerateRefreshToken()
    //{
    //    var randomNumber = new byte[32];
    //    using var rng = RandomNumberGenerator.Create();
    //    rng.GetBytes(randomNumber);
    //    return Convert.ToBase64String(randomNumber);
    //}
}


