using HW2.Models;

namespace HW2.Services;

public record struct TokenResult(string Token, int ExpiresInMinutes);

public interface ITokenService
{
    TokenResult Generate(User user);
}