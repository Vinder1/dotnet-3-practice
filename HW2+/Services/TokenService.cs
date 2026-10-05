using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HW2.Models;
using Microsoft.IdentityModel.Tokens;

namespace HW2.Services;

public class TokenService(IConfiguration configuration, ILogger<TokenService> logger) : ITokenService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<TokenService> _logger = logger;

    public TokenResult Generate(User user)
    {
        var configuredKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            _logger.LogError("Jwt:Key is not configured, token cannot be generated");
            throw new InvalidOperationException("Jwt:Key is not set.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuredKey));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        _logger.LogInformation(
            "JWT token generated for user {UserId}, expires in {ExpiresInMinutes} minutes",
            user.Id, expiresInMinutes);

        return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresInMinutes);
    }
}