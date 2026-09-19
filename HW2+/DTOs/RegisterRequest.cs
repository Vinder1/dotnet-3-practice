using System.ComponentModel.DataAnnotations;

namespace HW2.DTOs;

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username may only contain letters, digits and . _ -.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}