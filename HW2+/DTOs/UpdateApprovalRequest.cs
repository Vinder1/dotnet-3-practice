using System.ComponentModel.DataAnnotations;

namespace HW2.DTOs;

public class UpdateApprovalRequest : IValidatableObject
{
    [StringLength(200, MinimumLength = 3)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Description))
        {
            yield return new ValidationResult(
                "At least one field (title or description) must be provided.",
                [nameof(Title), nameof(Description)]);
        }
    }
}
