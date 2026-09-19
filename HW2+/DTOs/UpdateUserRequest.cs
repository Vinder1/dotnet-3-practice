using System.ComponentModel.DataAnnotations;

namespace HW2.DTOs;

public class UpdateUserRequest : IValidatableObject
{
    [StringLength(50, MinimumLength = 3)]
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Password))
        {
            yield return new ValidationResult(
                "At least one field (username, email or password) must be provided.",
                [nameof(Username), nameof(Email), nameof(Password)]);
        }
    }
}