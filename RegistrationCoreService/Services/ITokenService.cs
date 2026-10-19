using RegistrationCoreService.Models;

namespace RegistrationCoreService.Services;

public record struct TokenResult(string Token, int ExpiresInMinutes);

public interface ITokenService
{
    TokenResult Generate(User user);
}