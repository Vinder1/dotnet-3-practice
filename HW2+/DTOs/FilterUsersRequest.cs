using System.ComponentModel.DataAnnotations;

namespace HW2.DTOs;

public class FilterUsersRequest : IValidatableObject
{
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public DateTime? UpdatedFrom { get; init; }
    public DateTime? UpdatedTo { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreatedFrom is null && CreatedTo is null && UpdatedFrom is null && UpdatedTo is null)
        {
            yield return new ValidationResult(
                "At least one date filter is required (createdFrom, createdTo, updatedFrom or updatedTo).");
        }

        if (CreatedFrom is not null && CreatedTo is not null && CreatedFrom > CreatedTo)
        {
            yield return new ValidationResult(
                "createdFrom must not be later than createdTo.",
                [nameof(CreatedFrom), nameof(CreatedTo)]);
        }

        if (UpdatedFrom is not null && UpdatedTo is not null && UpdatedFrom > UpdatedTo)
        {
            yield return new ValidationResult(
                "updatedFrom must not be later than updatedTo.",
                [nameof(UpdatedFrom), nameof(UpdatedTo)]);
        }
    }
}