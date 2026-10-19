using System.ComponentModel.DataAnnotations;

namespace RegistrationCoreService.DTOs;

public class LoginRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}