namespace RegistrationCoreService.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; }
    public UserResponse User { get; set; } = new();
}