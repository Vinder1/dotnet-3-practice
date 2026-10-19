using System.ComponentModel.DataAnnotations;
using HW2.Models;

namespace HW2.DTOs;

public class FilterApprovalRequestsRequest : IValidatableObject
{
    public ApprovalStatus? Status { get; init; }
    public int? RequesterUserId { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreatedFrom is not null && CreatedTo is not null && CreatedFrom > CreatedTo)
        {
            yield return new ValidationResult(
                "createdFrom must not be later than createdTo.",
                [nameof(CreatedFrom), nameof(CreatedTo)]);
        }
    }
}
